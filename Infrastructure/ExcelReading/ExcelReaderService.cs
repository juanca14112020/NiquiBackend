using ClosedXML.Excel;
using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.Interfaces.Infrastructure;

namespace NiquiBackend.Infrastructure.ExcelReading;

public class ExcelReaderService : IExcelReaderService
{
    // Se asume fila 1 = encabezados: FirstName | LastName | Convenio | PhoneNumber
    public List<CustomerImportRowDto> ReadCustomers(Stream fileStream)
    {
        var result = new List<CustomerImportRowDto>();
        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheet(1);

        var rows = ws.RangeUsed()!.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            result.Add(new CustomerImportRowDto
            {
                RowNumber = row.RowNumber(),
                FirstName = row.Cell(1).GetString().Trim(),
                LastName = row.Cell(2).GetString().Trim(),
                Convenio = row.Cell(3).GetString().Trim(),
                PhoneNumber = row.Cell(4).GetString().Trim()
            });
        }
        return result;
    }
}