using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.Interfaces.Services;

namespace NiquiBackend.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Policy = "AnyAuthenticatedRole")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerBulkImportService _bulkImportService;

    public CustomerController(ICustomerService customerService, ICustomerBulkImportService bulkImportService)
    {
        _customerService = customerService;
        _bulkImportService = bulkImportService;
    }

    // Developer, SuperAdmin y Admin pueden ver el listado paginado y con filtros
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] CustomerQueryFilter filter)
    {
        var result = await _customerService.GetPagedAsync(filter);
        return Ok(result);
    }

    // Endpoint para obtener la lista de convenios disponibles para el dropdown
    [HttpGet("convenios")]
    public async Task<IActionResult> GetConvenios()
    {
        var convenios = await _customerService.GetConveniosAsync();
        return Ok(convenios);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        return customer is null ? NotFound() : Ok(customer);
    }

    // Developer, SuperAdmin y Admin pueden crear customers
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
    {
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var created = await _customerService.CreateAsync(dto, currentUserId, role);
        return CreatedAtAction(nameof(GetById), new { id = created.CustomerId }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateDto dto)
    {
        var success = await _customerService.UpdateAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _customerService.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }

    // BulkImport: SOLO SuperAdmin y Admin
    [HttpPost("bulk-import")]
    [Authorize(Policy = "CanBulkImportCustomers")]
    [ProducesResponseType(typeof(CustomerBulkImportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkImport(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Debe adjuntar un archivo .xlsx.");

        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        using var stream = file.OpenReadStream();
        var result = await _bulkImportService.ImportAsync(stream, userId, role);

        return Ok(result);
    }
}