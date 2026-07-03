using NiquiBackend.Application.DTOs.Admin;

namespace NiquiBackend.Application.Interfaces.Services;

public interface IAdminService
{
    Task<List<AdminResponseDto>> GetAllAsync();
    Task<AdminResponseDto> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, AdminUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}