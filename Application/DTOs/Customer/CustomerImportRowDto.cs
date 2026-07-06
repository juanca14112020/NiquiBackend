namespace NiquiBackend.Application.DTOs.Customer;

public class CustomerImportRowDto
{
    public int RowNumber { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Convenio { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}