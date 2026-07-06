namespace NiquiBackend.Application.DTOs.SuperAdmin;

public class SuperAdminSelfUpdateDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
}