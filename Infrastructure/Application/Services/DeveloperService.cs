using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.Developer;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Application.Services;

public class DeveloperService : IDeveloperService
{
    private readonly NiquiDbContext _context;

    public DeveloperService(NiquiDbContext context) => _context = context;

    public async Task<List<DeveloperResponseDto>> GetAllAsync()
        => await _context.Developers
            .Select(d => new DeveloperResponseDto
            {
                DeveloperId = d.DeveloperId,
                FirstName = d.FirstName,
                LastName = d.LastName,
                DocumentType = d.DocumentType,
                DocumentNumber = d.DocumentNumber,
                Email = d.Email,
                IsActive = d.IsActive
            }).ToListAsync();
    
    public async Task<DeveloperResponseDto> GetByIdAsync(Guid id)
    {
        var d = await _context.Developers.FindAsync(id);
        if (d is null) return null;

        return new DeveloperResponseDto
        {
            DeveloperId = d.DeveloperId,
            FirstName = d.FirstName,
            LastName = d.LastName,
            DocumentType = d.DocumentType,
            DocumentNumber = d.DocumentNumber,
            Email = d.Email,
            IsActive = d.IsActive
        };
    }

    public async Task<bool> UpdateAsync(Guid id, DeveloperUpdateDto dto)
    {
        var d = await _context.Developers.FindAsync(id);
        if (d is null) return false;

        d.FirstName = dto.FirstName;
        d.LastName = dto.LastName;
        d.DocumentType = dto.DocumentType;
        d.DocumentNumber = dto.DocumentNumber;
        d.Email = dto.Email;
        d.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var d = await _context.Developers.FindAsync(id);
        if (d is null) return false;

        _context.Developers.Remove(d);
        await _context.SaveChangesAsync();
        return true;
    }
}