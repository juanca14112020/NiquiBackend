using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using NiquiBackend.Application.DTOs.Auth;
using NiquiBackend.Application.Interfaces.Services;

namespace NiquiBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController (IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null) 
            return Unauthorized(new { message = "Credenciales invalidas." });

        return Ok(result);
    }

    [HttpPost("Register-developer-temp")]
    public async Task<IActionResult> RegisterDeveloperTemp([FromBody] RegisterRequestDto request)
    {
        try
        {
            var id = await _authService.RegisterDeveloperAsync(request);
            return Ok(new { message = "Developer creado. ", id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new {message = ex.Message });
        }
    }

    [HttpPost("register-superadmin-temp")]
    public async Task<IActionResult> RegisterSyperAdminTemp([FromBody] RegisterRequestDto request, [FromQuery]  Guid createdByDeveloperId)
    {
        try
        {
            var id = await _authService.RegisterSuperAdminAsync(request, createdByDeveloperId);
            return Ok(new {message = "SuperAdmin creado. ", id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("register-admin-temp")]
    public async Task<IActionResult> RegisterAdminTemp (
        [FromBody] RegisterRequestDto request,
        [FromQuery] Guid? createdByDeveloperId,
        [FromQuery] Guid? createdBySuperAdminId)
    {
        try
        {
            var id = await _authService.RegisterAdminAsync(request, createdByDeveloperId, createdBySuperAdminId);
            return Ok(new { message = "Admin creado. ", id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }
}