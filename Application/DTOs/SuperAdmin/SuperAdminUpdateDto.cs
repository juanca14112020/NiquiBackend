namespace NiquiBackend.Application.DTOs.SuperAdmin;

public class SuperAdminUpdateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public bool IsActive { get; set; }
}