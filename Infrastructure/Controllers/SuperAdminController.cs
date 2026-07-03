using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiquiBackend.Application.DTOs.SuperAdmin;
using NiquiBackend.Application.Interfaces.Services;

namespace NiquiBackend.Controllers;

[ApiController]
[Route("api/superadmins")]
[Authorize(Policy = "DeveloperOrSuperAdmin")]
public class SuperAdminController : ControllerBase
{
    private readonly ISuperAdminService _superAdminService;

    public SuperAdminController(ISuperAdminService superAdminService) => _superAdminService = superAdminService;

    //Solo developer puede listar a todos los SuperAdmins
    [HttpGet]
    [Authorize(Policy = "DeveloperOnly")]
    public async Task<IActionResult> GetAll() => Ok(await _superAdminService.GetAllAsync());

    //Developer ve cualquiera; SuperAdmin solo puede ver su propio registro
    [HttpGet("id:guid")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (role == "SuperAdmin" && currentUserId != id)
            return Forbid();

        var superAdmin = await _superAdminService.GetByIdAsync(id);
        return superAdmin is null ? NotFound() : Ok(superAdmin);
    }

    //Developer edita cualquiera; SuperAdmin solo su propio registro
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> update(Guid id, [FromBody] SuperAdminUpdateDto dto)
    {
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (role == "SuperAdmin" && currentUserId != id)
            return Forbid();
        
        var success = await _superAdminService.UpdateAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    //Solo Developer puede eliminar SuperAdmins
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "DeveloperOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _superAdminService.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}