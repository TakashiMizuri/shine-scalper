using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;
using ShineScalper.Core.Services;

namespace ShineScalper.Bybit;

public sealed class BybitMarketDataService : BackgroundService, IMarketDataStream
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IExchangeClient _exchangeClient;
    private readonly TradingStateCache _cache;
    private readonly TradingOptions _trading;
    private readonly BybitOptions _bybit;
    private readonly ILogger<BybitMarketDataService> _logger;
    private ClientWebSocket? _ws;

    public event Action<Candle>? CandleUpdated;
    public event Action<string, decimal>? TickerUpdated;

    public bool IsConnected => _cache.MarketDataConnected;

    public BybitMarketDataService(
        IServiceScopeFactory scopeFactory,
        IExchangeClient exchangeClient,
        TradingStateCache cache,
        IOptions<TradingOptions> trading,
        IOptions<BybitOptions> bybit,
        ILogger<BybitMarketDataService> logger)
    {
        _scopeFactory = scopeFactory;
        _exchangeClient = exchangeClient;
        _cache = cache;
        _trading = trading.Value;
        _bybit = bybit.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapped = false;
        var delay = TimeSpan.FromSeconds(2);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!bootstrapped)
                {
                    await BootstrapHistoryAsync(stoppingToken);
                    bootstrapped = true;
                }

                await RunWebSocketAsync(stoppingToken);
                delay = TimeSpan.FromSeconds(2);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _cache.SetMarketDataConnected(false);
                _logger.LogWarning(ex, "Bybit market data error, retry in {Delay}s", delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
            }
        }
    }

    private async Task BootstrapHistoryAsync(CancellationToken ct)
    {
        var symbol = _trading.Symbol;
        foreach (var interval in new[] { "5", "1" })
        {
            var candles = await _exchangeClient.GetKlinesAsync(symbol, interval, 1000, ct);
            foreach (var candle in candles)
            {
                await UpsertCandleAsync(CloneCandle(candle, true), ct);
            }
            _logger.LogInformation("Bootstrapped {Count} {Interval}m candles for {Symbol}", candles.Count, interval, symbol);
        }
    }

    private async Task RunWebSocketAsync(CancellationToken ct)
    {
        _ws?.Dispose();
        _ws = new ClientWebSocket();

        await _ws.ConnectAsync(new Uri(_bybit.WebSocketUrl), ct);
        _cache.SetMarketDataConnected(true);

        var symbol = _trading.Symbol;
        var subscribe = JsonSerializer.Serialize(new
        {
            op = "subscribe",
            args = new[]
            {
                $"kline.5.{symbol}",
                $"kline.1.{symbol}",
                $"tickers.{symbol}"
            }
        });

        await _ws.SendAsync(Encoding.UTF8.GetBytes(subscribe), WebSocketMessageType.Text, true, ct);
        _logger.LogInformation("Subscribed to Bybit WS for {Symbol}", symbol);

        var buffer = new byte[8192];
        while (_ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await _ws.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _cache.SetMarketDataConnected(false);
                    return;
                }
                ms.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);

            var json = Encoding.UTF8.GetString(ms.ToArray());
            await ProcessMessageAsync(json, ct);
        }
    }

    private async Task ProcessMessageAsync(string json, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("topic", out var topicEl))
            return;

        var topic = topicEl.GetString() ?? "";
        if (!root.TryGetProperty("data", out var data))
            return;

        if (topic.StartsWith("tickers.", StringComparison.Ordinal))
        {
            if (data.TryGetProperty("lastPrice", out var lastEl) &&
                decimal.TryParse(lastEl.GetString(), out var last))
            {
                _cache.SetLastPrice(last);
                TickerUpdated?.Invoke(_trading.Symbol, last);
                using (var scope = _scopeFactory.CreateScope())
                {
                    await scope.ServiceProvider.GetRequiredService<ITradingEventPublisher>()
                        .PublishTickerAsync(_trading.Symbol, last, ct);
                }
            }
            return;
        }

        if (!topic.StartsWith("kline.", StringComparison.Ordinal))
            return;

        var parts = topic.Split('.');
        if (parts.Length < 3) return;
        var interval = parts[1];
        var symbol = parts[2];

        JsonElement kline = data;
        if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0)
            kline = data[0];

        if (!kline.TryGetProperty("start", out var startEl)) return;

        var candle = new Candle
        {
            Symbol = symbol,
            Interval = interval,
            OpenTimeMs = startEl.ValueKind == JsonValueKind.Number
                ? startEl.GetInt64()
                : long.Parse(startEl.GetString()!),
            Open = ParseDec(kline, "open"),
            High = ParseDec(kline, "high"),
            Low = ParseDec(kline, "low"),
            Close = ParseDec(kline, "close"),
            Volume = ParseDec(kline, "volume"),
            IsClosed = kline.TryGetProperty("confirm", out var confirm) && confirm.GetBoolean()
        };

        await UpsertCandleAsync(candle, ct);
        CandleUpdated?.Invoke(candle);
    }

    private async Task UpsertCandleAsync(Candle candle, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ICandleRepository>().UpsertAsync(candle, ct);
        await sp.GetRequiredService<ITradingEventPublisher>().PublishCandleAsync(candle, ct);
    }

    private static decimal ParseDec(JsonElement el, string prop) =>
        decimal.Parse(el.GetProperty(prop).GetString()!, System.Globalization.CultureInfo.InvariantCulture);

    private static Candle CloneCandle(Candle c, bool isClosed) => new()
    {
        Symbol = c.Symbol,
        Interval = c.Interval,
        OpenTimeMs = c.OpenTimeMs,
        Open = c.Open,
        High = c.High,
        Low = c.Low,
        Close = c.Close,
        Volume = c.Volume,
        IsClosed = isClosed
    };

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _cache.SetMarketDataConnected(false);
        if (_ws?.State == WebSocketState.Open)
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "shutdown", cancellationToken);
        _ws?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
