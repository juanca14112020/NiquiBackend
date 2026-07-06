namespace NiquiBackend.Application.Interfaces.Infrastructure;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string email, string fullName, string role);
}