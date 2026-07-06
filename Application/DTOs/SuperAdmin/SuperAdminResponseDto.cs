namespace NiquiBackend.Application.DTOs.SuperAdmin;

public class SuperAdminResponseDto
{
    public Guid SuperAdminId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string Email {get; set; } = null!;
    public bool IsActive {get; set; }
}