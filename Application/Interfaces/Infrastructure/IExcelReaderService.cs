using NiquiBackend.Application.DTOs.Customer;

namespace NiquiBackend.Application.Interfaces.Infrastructure;

public interface IExcelReaderService
{
    List<CustomerImportRowDto> ReadCustomers(Stream fileStream);
}