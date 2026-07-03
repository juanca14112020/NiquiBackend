using NiquiBackend.Application.DTOs.SuperAdmin;

namespace NiquiBackend.Application.Interfaces.Services;

public interface ISuperAdminService
{
    Task<List<SuperAdminResponseDto>> GetAllAsync();
    Task<SuperAdminResponseDto?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, SuperAdminUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}