using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiquiBackend.Application.DTOs.Admin;
using NiquiBackend.Application.Interfaces.Services;

namespace NiquiBackend.Controllers;

[ApiController]
[Route("api/admins")]
[Authorize(Policy = "AnyAuthenticatedRole")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    
    public AdminController(IAdminService adminService) => _adminService = adminService;

    //Developer y SuperAdmin listan todos; Admin no tiene acceso a la lista completa 
    [HttpGet]
    [Authorize(Policy = "DeveloperOrSuperAdmin")]
    public async Task<IActionResult> GetAll() => Ok(await _adminService.GetAllAsync());

    //Developer/SuperAdmin ven cualquiera; Admin solo su propio registro
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (role == "Admin" && currentUserId != id) 
            return Forbid();

        var admin = await _adminService.GetByIdAsync(id);
        return admin is null ? NotFound() : Ok(admin);
    }

    //Developer/SuperAdmin editan cualquiera; Admin solo su propio registro
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AdminUpdateDto dto)
    {
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (role == "Admin" && currentUserId != id)
            return Forbid();

        var success = await _adminService.UpdateAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    //Solo Developer o SuperAdmin pueden eliminar Admins, EL propio Admin NUNCA puede.
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "DeveloperOrSuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _adminService.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}