namespace NiquiBackend.Application.DTOs.Customer;

public class CustomerUpdateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } =  null!;
    public string Convenio { get; set; } = null!;
    public string PhoneNumber { get; set;} = null!;
    public bool IsApproved { get; set; }
    public bool IsCalled {get; set; }
}