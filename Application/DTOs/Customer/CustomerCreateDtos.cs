namespace NiquiBackend.Application.DTOs.Customer;


public class CustomerCreateDto
{
    public string FullName { get; set; } = null!;
    public string Convenio { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}