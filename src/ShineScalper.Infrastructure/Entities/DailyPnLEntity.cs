namespace ShineScalper.Infrastructure.Entities;

public sealed class DailyPnLEntity
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal PnL { get; set; }
}
