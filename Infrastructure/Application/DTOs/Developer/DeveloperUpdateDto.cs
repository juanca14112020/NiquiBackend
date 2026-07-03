namespace NiquiBackend.Application.DTOs.Developer;

public class DeveloperUpdateDto
{   
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
}