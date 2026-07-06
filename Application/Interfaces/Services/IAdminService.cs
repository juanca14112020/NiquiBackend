using NiquiBackend.Application.DTOs.Admin;

namespace NiquiBackend.Application.Interfaces.Services;

public interface IAdminService
{
    Task<List<AdminResponseDto>> GetAllAsync();
    Task<AdminResponseDto> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, AdminUpdateDto dto);
    //Edición restringida usada por el porbio admin sobre si mismo: solo nombre y apellido
    Task<bool> UpdateOwnProfileAsync(Guid id, AdminSelfUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}