namespace NiquiBackend.Application.DTOs.MassCalls;

public class StartMassCallDto
{
  public string Convenio { get; set; } = null!;
  public int TargetCalls { get; set; }
  public int TimeLimitMinutes { get; set; }
}