using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.Auth;
using NiquiBackend.Application.Interfaces.Infrastructure;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Application.Services;

public class AuthService : IAuthService
{
    private readonly NiquiDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(NiquiDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var developer = await _context.Developers
            .FirstOrDefaultAsync(d => d.Email == request.Email && d.IsActive);

        if (developer != null && _passwordHasher.Verify(request.Password, developer.PasswordHash))
        {
            var (token, expiresAt) = _tokenGenerator.GenerateToken(
                developer.DeveloperId, developer.Email, $"{developer.FirstName} {developer.LastName}", "Developer");

            return BuildResponse(token, expiresAt, developer.DeveloperId, "Developer",
                $"{developer.FirstName} {developer.LastName}", developer.Email);
        }

        var superAdmin = await _context.SuperAdmins
            .FirstOrDefaultAsync(s => s.Email == request.Email && s.IsActive);

        if (superAdmin != null && _passwordHasher.Verify(request.Password, superAdmin.PasswordHash))
        {
            var (token, expiresAt) = _tokenGenerator.GenerateToken(
                superAdmin.SuperAdminId, superAdmin.Email, $"{superAdmin.FirstName} {superAdmin.LastName}", "SuperAdmin");

            return BuildResponse(token, expiresAt, superAdmin.SuperAdminId, "SuperAdmin",
                $"{superAdmin.FirstName} {superAdmin.LastName}", superAdmin.Email);
        }

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email && a.IsActive);

        if (admin != null && _passwordHasher.Verify(request.Password, admin.PasswordHash))
        {
            var (token, expiresAt) = _tokenGenerator.GenerateToken(
                admin.AdminId, admin.Email, $"{admin.FirstName} {admin.LastName}", "Admin");

            return BuildResponse(token, expiresAt, admin.AdminId, "Admin",
                $"{admin.FirstName} {admin.LastName}", admin.Email);
        }

        return null;
    }

    // ===== Endpoints temporales de registro (quitar o proteger antes de produccion) =====

    public async Task<Guid> RegisterDeveloperAsync(RegisterRequestDto request)
    {
        var exists = await _context.Developers.AnyAsync(d => d.Email == request.Email);
        if (exists) throw new InvalidOperationException("Ya existe un Developer con ese email.");

        var developer = new Developer
        {
            DeveloperId = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true
        };

        _context.Developers.Add(developer);
        await _context.SaveChangesAsync();
        return developer.DeveloperId;
    }

    public async Task<Guid> RegisterSuperAdminAsync(RegisterRequestDto request, Guid createdByDeveloperId)
    {
        var developerExists = await _context.Developers.AnyAsync(d => d.DeveloperId == createdByDeveloperId);
        if (!developerExists) throw new InvalidOperationException("El Developer indicado no existe.");

        var exists = await _context.SuperAdmins.AnyAsync(s => s.Email == request.Email);
        if (exists) throw new InvalidOperationException("Ya existe un SuperAdmin con ese email.");

        var superAdmin = new SuperAdmin
        {
            SuperAdminId = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            CreatedByDeveloperId = createdByDeveloperId
        };

        _context.SuperAdmins.Add(superAdmin);
        await _context.SaveChangesAsync();
        return superAdmin.SuperAdminId;
    }

    public async Task<Guid> RegisterAdminAsync(RegisterRequestDto request, Guid? createdByDeveloperId, Guid? createdBySuperAdminId)
    {
        if (createdByDeveloperId is null && createdBySuperAdminId is null)
            throw new InvalidOperationException("Debe indicar quien crea el Admin (Developer o SuperAdmin).");

        var exists = await _context.Admins.AnyAsync(a => a.Email == request.Email);
        if (exists) throw new InvalidOperationException("Ya existe un Admin con ese email.");

        var admin = new Admin
        {
            AdminId = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            CreatedByDeveloperId = createdByDeveloperId,
            CreatedBySuperAdminId = createdBySuperAdminId
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();
        return admin.AdminId;
    }

    private static LoginResponseDto BuildResponse(string token, DateTime expiresAt, Guid userId, string role, string fullName, string email)
        => new()
        {
            Token = token,
            Role = role,
            UserId = userId,
            FullName = fullName,
            Email = email,
            ExpiresAt = expiresAt
        };
}
