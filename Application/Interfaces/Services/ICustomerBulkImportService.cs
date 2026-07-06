using NiquiBackend.Application.DTOs.Customer;

namespace NiquiBackend.Application.Interfaces.Services;

public interface ICustomerBulkImportService
{
    Task<CustomerBulkImportResultDto> ImportAsync(Stream file, Guid currentUserId, string currentUserRole);
}