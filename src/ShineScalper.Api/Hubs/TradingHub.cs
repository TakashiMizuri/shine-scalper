using Microsoft.AspNetCore.SignalR;

namespace ShineScalper.Api.Hubs;

public sealed class TradingHub : Hub
{
    public const string HubPath = "/hubs/trading";
}
