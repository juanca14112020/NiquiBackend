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
            query = query.Where(c => c.FirstName.ToLower().Contains(searchTerm) || 
                                     c.LastName.ToLower().Contains(searchTerm));
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
                FirstName = c.FirstName,
                LastName = c.LastName,
                Convenio = c.Convenio,
                PhoneNumber = c.PhoneNumber,
                IsApproved = c.IsApproved,
                IsCalled = c.IsCalled,
                CreatedBySuperAdminId = c.CreatedBySuperAdminId,
                CreatedByAdminId = c.CreatedByAdminId,
                CreatedAt = c.CreatedAt,
                // Validamos solo Admin y SuperAdmin
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
            FirstName = c.FirstName,
            LastName = c.LastName,
            Convenio = c.Convenio,
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
            FirstName = dto.FirstName,
            LastName = dto.LastName,
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
            FirstName = customer.FirstName,
            LastName = customer.LastName,
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

        c.FirstName = dto.FirstName;
        c.LastName = dto.LastName;
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
}