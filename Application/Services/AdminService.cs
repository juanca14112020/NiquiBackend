using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.Admin;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Application.Services;

public class AdminService : IAdminService
{
    private readonly NiquiDbContext _context;

    public AdminService(NiquiDbContext context) => _context = context;

    public async Task<List<AdminResponseDto>> GetAllAsync()
        => await _context.Admins
            .Select(a => new AdminResponseDto
            {
                AdminId = a.AdminId,
                FirstName = a.FirstName,
                LastName = a.LastName,
                DocumentType = a.DocumentType,
                DocumentNumber = a.DocumentNumber,
                Email = a.Email,
                IsActive = a.IsActive
            }).ToListAsync();

    public async Task<AdminResponseDto?> GetByIdAsync(Guid id)
    {
        var a = await _context.Admins.FindAsync(id);
        if (a is null) return null;

        return new AdminResponseDto
        {
            AdminId = a.AdminId,
            FirstName = a.FirstName,
            LastName = a.LastName,
            DocumentType = a.DocumentType,
            DocumentNumber = a.DocumentNumber,
            Email = a.Email,
            IsActive = a.IsActive
        };
    }

    // Usado por Developer o SuperAdmin (ambos pueden cambiar IsActive). Sin Email.
    public async Task<bool> UpdateAsync(Guid id, AdminUpdateDto dto)
    {
        var a = await _context.Admins.FindAsync(id);
        if (a is null) return false;

        a.FirstName = dto.FirstName;
        a.LastName = dto.LastName;
        a.DocumentType = dto.DocumentType;
        a.DocumentNumber = dto.DocumentNumber;
        a.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    // El propio Admin editando su perfil: SOLO nombre y apellido.
    public async Task<bool> UpdateOwnProfileAsync(Guid id, AdminSelfUpdateDto dto)
    {
        var a = await _context.Admins.FindAsync(id);
        if (a is null) return false;

        a.FirstName = dto.FirstName;
        a.LastName = dto.LastName;
        // Email, DocumentNumber, DocumentType e IsActive NO se tocan aca.

        await _context.SaveChangesAsync();
        return true;
    }

    // El Admin NUNCA puede eliminar (ni a si mismo). Solo Developer/SuperAdmin, restringido en el controller.
    public async Task<bool> DeleteAsync(Guid id)
    {
        var a = await _context.Admins.FindAsync(id);
        if (a is null) return false;

        _context.Admins.Remove(a);
        await _context.SaveChangesAsync();
        return true;
    }
}
