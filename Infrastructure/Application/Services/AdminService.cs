using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.Admin;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Application.Services;

public class AdminService : IAdminService
{
    private readonly NiquiDbContext _context;

    public AdminService(NiquiDbContext context ) => _context = context;

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
    
    public async Task<AdminResponseDto> GetByIdAsync(Guid id)
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

    public async Task<bool> UpdateAsync(Guid id, AdminUpdateDto dto)
    {
        var a = await _context.Admins.FindAsync(id);
        if (a is null) return false;

        a.FirstName = dto.FirstName;
        a.LastName = dto.LastName;
        a.DocumentType = dto.DocumentType;
        a.DocumentNumber = dto.DocumentNumber;
        a.Email = dto.Email;
        a.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var a = await _context.Admins.FindAsync(id);
        if (a is null) return false;

        _context.Admins.Remove(a);
        await _context.SaveChangesAsync();
        return true;
    }
}