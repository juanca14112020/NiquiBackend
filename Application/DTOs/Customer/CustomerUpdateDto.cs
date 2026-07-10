namespace NiquiBackend.Application.DTOs.Customer;

public class CustomerUpdateDto
{
    public string FullName { get; set; } = null!;
    public string Convenio { get; set; } = null!;
    public string PhoneNumber { get; set;} = null!;
    public bool IsApproved { get; set; }
    public bool IsCalled {get; set; }
}