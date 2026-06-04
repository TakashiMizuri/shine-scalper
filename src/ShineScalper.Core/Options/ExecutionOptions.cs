namespace ShineScalper.Core.Options;

public sealed class ExecutionOptions
{
    public const string SectionName = "Execution";

    public decimal PaperSlippageBps { get; set; } = 5m;
    public decimal FeeTakerBps { get; set; } = 5.5m;
}
