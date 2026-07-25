namespace NiquiBackend.Infrastructure.Persistence.Generated;

public class MassCallExecution
{
    public Guid Id { get; set; }
    public string Convenio { get; set; } = null!;
    public int TargetCalls { get; set; }
    public int CallsMade { get; set; }
    public int SkippedUnverified { get; set; }
    public int TimeLimitMinutes { get; set; }
    public string Status { get; set; } = "Running";
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}