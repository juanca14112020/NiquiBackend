using NiquiBackend.Application.DTOs.Auth;
using NiquiBackend.Application.DTOS.Auth;

namespace NiquiBackend.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    //Temporales -solo para pruebas

    Task<Guid> RegisterDeveloperAsync(RegisterRequestDto request);
    Task<Guid> RegisterSuperAdminAsync(RegisterRequestDto request, Guid createdByDeveloperId);
    Task<Guid> RegisterAdminAsync(RegisterRequestDto request, Guid? createdByDeveloperId, Guid? createdBySuperAdminId);
}