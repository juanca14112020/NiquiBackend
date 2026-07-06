using System.Data;

namespace NiquiBackend.Application.Interfaces.Infrastructure;

public interface IBulkInsertService
{
    Task BulkInsertAsync(DataTable table, string destinationTable);
}