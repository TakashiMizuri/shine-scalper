namespace ShineScalper.Core.Options;

public sealed class BybitOptions
{
    public const string SectionName = "Bybit";

    public string BaseUrl { get; set; } = "https://api.bybit.com";
    public string WebSocketUrl { get; set; } = "wss://stream.bybit.com/v5/public/linear";
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
}
