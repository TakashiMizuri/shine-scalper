using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;

namespace ShineScalper.Bybit;

public sealed class BybitRestClient(HttpClient http, IOptions<BybitOptions> options) : IExchangeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<Candle>> GetKlinesAsync(string symbol, string interval, int limit, CancellationToken ct = default)
    {
        var url = $"{options.Value.BaseUrl}/v5/market/kline?category=linear&symbol={symbol}&interval={interval}&limit={limit}";
        using var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<BybitKlineResponse>(JsonOptions, ct);
        if (payload?.Result?.List is null)
            return [];

        return payload.Result.List
            .Select(row => MapKline(symbol, interval, row))
            .OrderBy(c => c.OpenTimeMs)
            .ToList();
    }

    private static Candle MapKline(string symbol, string interval, string[] row)
    {
        // [startTime, open, high, low, close, volume, turnover]
        return new Candle
        {
            Symbol = symbol,
            Interval = interval,
            OpenTimeMs = long.Parse(row[0], CultureInfo.InvariantCulture),
            Open = decimal.Parse(row[1], CultureInfo.InvariantCulture),
            High = decimal.Parse(row[2], CultureInfo.InvariantCulture),
            Low = decimal.Parse(row[3], CultureInfo.InvariantCulture),
            Close = decimal.Parse(row[4], CultureInfo.InvariantCulture),
            Volume = decimal.Parse(row[5], CultureInfo.InvariantCulture),
            IsClosed = true
        };
    }

    private sealed class BybitKlineResponse
    {
        public KlineResult? Result { get; set; }
    }

    private sealed class KlineResult
    {
        public List<string[]>? List { get; set; }
    }
}
