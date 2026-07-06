using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NiquiBackend.Application.Interfaces.Infrastructure;

namespace NiquiBackend.Infrastructure.BulkOperations;

public class SqlBulkInsertService : IBulkInsertService
{
    private readonly string _connectionString;
    
    public SqlBulkInsertService(IConfiguration config)
        => _connectionString = config.GetConnectionString("NiquiConnection")!;
    
    public async Task BulkInsertAsync(DataTable table, string destinationTable)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = destinationTable,
            BatchSize = 1000
        };

        foreach (DataColumn column in table.Columns)
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

            await bulkCopy.WriteToServerAsync(table);
    }

}