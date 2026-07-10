using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.DTOs.Common;

namespace NiquiBackend.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<PagedResponse<CustomerResponseDto>> GetPagedAsync(CustomerQueryFilter filter);
    Task<List<string>> GetConveniosAsync();
    Task<CustomerResponseDto?> GetByIdAsync(Guid id);
    Task<CustomerResponseDto> CreateAsync(CustomerCreateDto dto, Guid currentUserId, string currentUserRole);
    Task<bool> UpdateAsync(Guid id, CustomerUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<int> DeleteAllAsync();

    Task<byte[]> ExportToExcelAsync(CustomerQueryFilter filter);
}