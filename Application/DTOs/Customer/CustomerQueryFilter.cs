namespace NiquiBackend.Application.DTOs.Customer;

public class CustomerQueryFilter
{
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 20;
  public string? Search { get; set; } //Buscar por nombre y apellido
  public string? Convenio { get; set; }
  public bool? IsCalled { get; set; }
  public bool? IsApproved { get; set; }
}