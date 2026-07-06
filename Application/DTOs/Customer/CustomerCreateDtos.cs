namespace NiquiBackend.Application.DTOs.Customer;


public class CustomerCreateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Convenio { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}