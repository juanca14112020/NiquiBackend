using NiquiBackend.Application.DTOs.Customer;

namespace NiquiBackend.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<List<CustomerResponseDto>> GetAllAsync();
    Task<CustomerResponseDto?> GetByIdAsync(Guid id);
    Task<CustomerResponseDto> CreateAsync(CustomerCreateDto dto, Guid currentUserId, string currentUserRole);
    Task<bool> UpdateAsync(Guid id, CustomerUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}