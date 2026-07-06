using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.SuperAdmin;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Application.Services;

public class SuperAdminService : ISuperAdminService
{
    private readonly NiquiDbContext _context;

    public SuperAdminService(NiquiDbContext context) => _context = context;

    public async Task<List<SuperAdminResponseDto>> GetAllAsync()
        => await _context.SuperAdmins
            .Select(s => new SuperAdminResponseDto
            {
                SuperAdminId = s.SuperAdminId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                DocumentType = s.DocumentType,
                DocumentNumber = s.DocumentNumber,
                Email = s.Email,
                IsActive = s.IsActive
            }).ToListAsync();

    public async Task<SuperAdminResponseDto?> GetByIdAsync(Guid id)
    {
        var s = await _context.SuperAdmins.FindAsync(id);
        if (s is null) return null;

        return new SuperAdminResponseDto
        {
            SuperAdminId = s.SuperAdminId,
            FirstName = s.FirstName,
            LastName = s.LastName,
            DocumentType = s.DocumentType,
            DocumentNumber = s.DocumentNumber,
            Email = s.Email,
            IsActive = s.IsActive
        };
    }

    // SOLO el Developer llega aca (controller lo restringe). Incluye IsActive.
    // El Email nunca se toca.
    public async Task<bool> UpdateAsync(Guid id, SuperAdminUpdateDto dto)
    {
        var s = await _context.SuperAdmins.FindAsync(id);
        if (s is null) return false;

        s.FirstName = dto.FirstName;
        s.LastName = dto.LastName;
        s.DocumentType = dto.DocumentType;
        s.DocumentNumber = dto.DocumentNumber;
        s.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    // El propio SuperAdmin editando su perfil: NO puede tocar IsActive ni Email.
    public async Task<bool> UpdateOwnProfileAsync(Guid id, SuperAdminSelfUpdateDto dto)
    {
        var s = await _context.SuperAdmins.FindAsync(id);
        if (s is null) return false;

        s.FirstName = dto.FirstName;
        s.LastName = dto.LastName;
        s.DocumentType = dto.DocumentType;
        s.DocumentNumber = dto.DocumentNumber;
        // IsActive y Email deliberadamente NO se tocan aca.

        await _context.SaveChangesAsync();
        return true;
    }

    // Solo el Developer puede eliminar (restringido en el controller).
    public async Task<bool> DeleteAsync(Guid id)
    {
        var s = await _context.SuperAdmins.FindAsync(id);
        if (s is null) return false;

        _context.SuperAdmins.Remove(s);
        await _context.SaveChangesAsync();
        return true;
    }
}
