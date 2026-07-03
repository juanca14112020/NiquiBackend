using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiquiBackend.Application.DTOs.Developer;
using NiquiBackend.Application.Interfaces.Services;

namespace NiquiBackend.Controllers;

[ApiController]
[Route("api/developers")]
[Authorize(policy: "DeveloperOnly")]
public class DeveloperController : ControllerBase
{
    private readonly IDeveloperService _developerService;
    
    public DeveloperController(IDeveloperService developerService) => _developerService = developerService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _developerService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var developer = await _developerService.GetByIdAsync(id);
        return developer is null ? NotFound() : Ok(developer);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] DeveloperUpdateDto dto)
    {
        var success = await _developerService.UpdateAsync(id, dto);
        return success ? NoContent() : NotFound();
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _developerService.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}