using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly NiquiDbContext _context;

    public CustomerService(NiquiDbContext context) => _context = context;

    public async Task<List<CustomerResponseDto>> GetAllAsync()
        => await _context.Customers
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
                CreatedAt = c.CreatedAt
            }).ToListAsync();
    
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