namespace NiquiBackend.Application.DTOs.Auth;

public class RegisterRequestDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password {get; set; } = null!;
}