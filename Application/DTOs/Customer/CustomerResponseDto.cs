namespace NiquiBackend.Application.DTOs.Customer;

public class CustomerResponseDto
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Convenio { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public bool IsApproved { get; set; }
    public bool IsCalled { get; set; }
    public Guid? CreatedBySuperAdminId { get; set; }
    public Guid? CreatedByAdminId { get; set; }
    public Guid? CreatedByDeveloperId { get; set; }
    public DateTime CreatedAt { get; set; }
    
}