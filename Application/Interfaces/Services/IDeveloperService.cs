using NiquiBackend.Application.DTOs.Developer;

namespace NiquiBackend.Application.Interfaces.Services;

public interface IDeveloperService
{
    Task<List<DeveloperResponseDto>> GetAllAsync();
    Task<DeveloperResponseDto?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, DeveloperUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}