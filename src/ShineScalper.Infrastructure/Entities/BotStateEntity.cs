namespace ShineScalper.Infrastructure.Entities;

public sealed class BotStateEntity
{
    public int Id { get; set; } = 1;
    public decimal Equity { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
