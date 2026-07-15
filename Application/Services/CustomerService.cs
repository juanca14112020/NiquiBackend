using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.Common;
using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly NiquiDbContext _context;

    public CustomerService(NiquiDbContext context) => _context = context;

    public async Task<PagedResponse<CustomerResponseDto>> GetPagedAsync(CustomerQueryFilter filter)
    {
        var query = _context.Customers.AsQueryable();

        // 1. Aplicar Filtros
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            query = query.Where(c => c.FullName.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(filter.Convenio))
        {
            query = query.Where(c => c.Convenio == filter.Convenio);
        }

        if (filter.IsCalled.HasValue)
        {
            query = query.Where(c => c.IsCalled == filter.IsCalled.Value);
        }

        if (filter.IsApproved.HasValue)
        {
            query = query.Where(c => c.IsApproved == filter.IsApproved.Value);
        }

        // 2. Contar el total de registros filtrados
        var totalRecords = await query.CountAsync();

        // 3. Paginación y Proyección
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CustomerResponseDto
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Convenio = c.Convenio,
                PhoneNumber = c.PhoneNumber,
                IsApproved = c.IsApproved,
                IsCalled = c.IsCalled,
                CreatedBySuperAdminId = c.CreatedBySuperAdminId,
                CreatedByAdminId = c.CreatedByAdminId,
                CreatedAt = c.CreatedAt,
                CreatedByName = c.CreatedByAdmin != null ? c.CreatedByAdmin.FirstName + " " + c.CreatedByAdmin.LastName :
                                c.CreatedBySuperAdmin != null ? c.CreatedBySuperAdmin.FirstName + " " + c.CreatedBySuperAdmin.LastName : "Desconocido"
            })
            .ToListAsync();

        return new PagedResponse<CustomerResponseDto>(items, totalRecords, filter.Page, filter.PageSize);
    }

    public async Task<List<string>> GetConveniosAsync()
    {
        return await _context.Customers
            .Where(c => !string.IsNullOrEmpty(c.Convenio))
            .Select(c => c.Convenio!)
            .Distinct()
            .ToListAsync();
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(Guid id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c is null) return null;

        return new CustomerResponseDto
        {
            CustomerId = c.CustomerId,
            FullName = c.FullName,
            Convenio = c.Convenio,
            PhoneNumber = c.PhoneNumber,
            IsApproved = c.IsApproved,
            IsCalled = c.IsCalled,
            CreatedBySuperAdminId = c.CreatedBySuperAdminId,
            CreatedByAdminId = c.CreatedByAdminId,
            CreatedAt = c.CreatedAt
        };
    }
    
    public async Task<CustomerResponseDto> CreateAsync(CustomerCreateDto dto, Guid currentUserId, string currentUserRole)
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            FullName = dto.FullName,
            Convenio = dto.Convenio,
            PhoneNumber = dto.PhoneNumber,
            IsApproved = false,
            IsCalled = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBySuperAdminId = currentUserRole == "SuperAdmin" ? currentUserId : null,
            CreatedByAdminId = currentUserRole == "Admin" ? currentUserId : null,
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerResponseDto
        {
            CustomerId = customer.CustomerId,
            FullName = customer.FullName,
            Convenio = customer.Convenio,
            PhoneNumber = customer.PhoneNumber,
            IsApproved = customer.IsApproved,
            IsCalled = customer.IsCalled,
            CreatedBySuperAdminId = customer.CreatedBySuperAdminId,
            CreatedByAdminId = customer.CreatedByAdminId,
            CreatedAt = customer.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(Guid id, CustomerUpdateDto dto)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c is null) return false;

        if (dto.IsApproved) 
        {
            dto.IsCalled = true;
        }
        if (!dto.IsCalled)
        {
            dto.IsApproved = false;
        }

        c.FullName = dto.FullName;
        c.Convenio = dto.Convenio;
        c.PhoneNumber = dto.PhoneNumber;
        
        c.IsApproved = dto.IsApproved;
        c.IsCalled = dto.IsCalled;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c is null) return false;

        _context.Customers.Remove(c);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteAllAsync()
    {
        return await _context.Customers.ExecuteDeleteAsync();
    }

    public async Task<byte[]> ExportToExcelAsync(CustomerQueryFilter filter)
    {
        var query = _context.Customers.AsQueryable();

        // Mismos filtros que GetPagedAsync, pero SIN Skip/Take (exporta todo lo que matchea)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            query = query.Where(c => c.FullName.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(filter.Convenio))
        {
            query = query.Where(c => c.Convenio == filter.Convenio);
        }

        if (filter.IsCalled.HasValue)
        {
            query = query.Where(c => c.IsCalled == filter.IsCalled.Value);
        }

        if (filter.IsApproved.HasValue)
        {
            query = query.Where(c => c.IsApproved == filter.IsApproved.Value);
        }

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerResponseDto
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Convenio = c.Convenio,
                PhoneNumber = c.PhoneNumber,
                IsApproved = c.IsApproved,
                IsCalled = c.IsCalled,
                CreatedAt = c.CreatedAt,
                CreatedByName = c.CreatedByAdmin != null ? c.CreatedByAdmin.FirstName + " " + c.CreatedByAdmin.LastName :
                                c.CreatedBySuperAdmin != null ? c.CreatedBySuperAdmin.FirstName + " " + c.CreatedBySuperAdmin.LastName : "Desconocido"
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Clientes");

        ws.Cell(1, 1).Value = "Nombre completo";
        ws.Cell(1, 2).Value = "Teléfono";
        ws.Cell(1, 3).Value = "Convenio";
        ws.Cell(1, 4).Value = "Llamado";
        ws.Cell(1, 5).Value = "Aceptó";
        ws.Cell(1, 6).Value = "Creado por";
        ws.Cell(1, 7).Value = "Fecha de creación";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a2130");
        headerRow.Style.Font.FontColor = XLColor.White;

        var row = 2;
        foreach (var c in items)
        {
            ws.Cell(row, 1).Value = c.FullName;
            ws.Cell(row, 2).Value = c.PhoneNumber;
            ws.Cell(row, 3).Value = c.Convenio;
            ws.Cell(row, 4).Value = c.IsCalled ? "Sí" : "No";
            ws.Cell(row, 5).Value = c.IsApproved ? "Sí" : "No";
            ws.Cell(row, 6).Value = c.CreatedByName;
            ws.Cell(row, 7).Value = c.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<bool> MarkAsCalledAsync(Guid customerId)
    {
        var updated = await _context.Customers
            .Where(c => c.CustomerId == customerId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsCalled, true));
        
        return updated > 0;
    }
}