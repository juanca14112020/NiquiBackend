using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.MassCalls;
using NiquiBackend.Infrastructure.Common;
using NiquiBackend.Infrastructure.MassCalls;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Controllers;

[ApiController]
[Route("api/mass-calls")]
[Authorize]
public class MassCallsController : ControllerBase
{
    private readonly NiquiDbContext _db;
    private readonly MassCallRunner _runner;

    public MassCallsController(NiquiDbContext db, MassCallRunner runner)
    {
        _db = db;
        _runner = runner;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartMassCallDto dto)
    {
        var execution = new MassCallExecution
        {
            Id = Guid.NewGuid(),
            Convenio = dto.Convenio,
            TargetCalls = dto.TargetCalls,
            TimeLimitMinutes = dto.TimeLimitMinutes,
            TimeSlot = dto.TimeSlot,
            Status = "Running",
            CreatedAt = DateTime.UtcNow
        };

        _db.MassCallExecutions.Add(execution);
        await _db.SaveChangesAsync();

        _runner.Start(execution.Id, dto.Convenio, dto.TargetCalls, dto.TimeLimitMinutes, dto.TimeSlot);

        return Ok(new { id = execution.Id });
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> Stop(Guid id)
    {
        await _runner.StopAsync(id, _db);
        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var records = await _db.MassCallExecutions
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var result = records.Select(x => new
        {
            x.Id,
            x.Convenio,
            x.TargetCalls,
            x.CallsMade,
            x.SkippedUnverified,
            x.TimeLimitMinutes,
            x.TimeSlot,
            x.Status,
            CreatedAt = ColombiaTimeHelper.ConvertToColombiaTime(x.CreatedAt),
            FinishedAt = x.FinishedAt.HasValue ? ColombiaTimeHelper.ConvertToColombiaTime(x.FinishedAt.Value) : (DateTime?)null
        });

        return Ok(result);
    }
}