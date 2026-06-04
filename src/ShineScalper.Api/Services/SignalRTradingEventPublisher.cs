using Microsoft.AspNetCore.SignalR;
using ShineScalper.Api.Dtos;
using ShineScalper.Api.Hubs;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;

namespace ShineScalper.Api.Services;

public sealed class SignalRTradingEventPublisher(IHubContext<TradingHub> hub) : ITradingEventPublisher
{
    public Task PublishCandleAsync(Candle candle, CancellationToken ct = default) =>
        hub.Clients.All.SendAsync("candle", CandleDto.From(candle), ct);

    public Task PublishLevelsAsync(string symbol, IReadOnlyList<PriceZone> levels, CancellationToken ct = default) =>
        hub.Clients.All.SendAsync("levels", new { symbol, zones = levels.Select(LevelDto.From) }, ct);

    public Task PublishSignalAsync(TradeSignal signal, CancellationToken ct = default) =>
        hub.Clients.All.SendAsync("signal", SignalDto.From(signal), ct);

    public Task PublishPositionAsync(Position position, CancellationToken ct = default) =>
        hub.Clients.All.SendAsync("position", PositionDto.From(position), ct);

    public Task PublishTradeAsync(Fill fill, CancellationToken ct = default) =>
        hub.Clients.All.SendAsync("trade", FillDto.From(fill), ct);

    public Task PublishTickerAsync(string symbol, decimal price, CancellationToken ct = default) =>
        hub.Clients.All.SendAsync("ticker", new { symbol, price, time = DateTime.UtcNow }, ct);
}
