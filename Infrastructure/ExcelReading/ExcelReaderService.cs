using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.Interfaces.Infrastructure;

namespace NiquiBackend.Infrastructure.ExcelReading;

public class ExcelReaderService : IExcelReaderService
{
    // Alias para nombre completo en una sola columna
    private static readonly string[] FullNameAliases =
        { "NOMBRE COMPLETO", "NOMBRE Y APELLIDO", "NOMBRES Y APELLIDOS", "FULLNAME", "NOMBRE COMPLETO CLIENTE" };

    // Alias para cuando el Excel trae Nombre y Apellido en columnas separadas (se concatenan)
    private static readonly string[] FirstNameAliases = { "NOMBRE", "NOMBRES", "PRIMER NOMBRE", "FIRSTNAME" };
    private static readonly string[] LastNameAliases = { "APELLIDO", "APELLIDOS", "LASTNAME" };

    private static readonly string[] ConvenioAliases = { "CONVENIO" };
    private static readonly string[] PhoneAliases =
        { "TELEFONO", "TELEFONOS", "CELULAR", "NUMERO", "NUMERO CELULAR", "TEL", "PHONE", "PHONENUMBER" };

    public List<CustomerImportRowDto> ReadCustomers(Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheet(1);

        var headerRow = ws.Row(1);
        var (fullNameCol, firstNameCol, lastNameCol, convenioCol, phoneCol) = MapColumns(headerRow);

        if (fullNameCol is null && (firstNameCol is null || lastNameCol is null))
        {
            throw new InvalidOperationException(
                "No se encontro una columna de 'Nombre completo', ni el par 'Nombre' + 'Apellido' en el Excel.");
        }

        if (convenioCol is null || phoneCol is null)
        {
            throw new InvalidOperationException(
                "No se encontraron en el Excel las columnas de Convenio y/o Telefono.");
        }

        var result = new List<CustomerImportRowDto>();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int rowNum = 2; rowNum <= lastRow; rowNum++)
        {
            var row = ws.Row(rowNum);
            if (row.IsEmpty()) continue;

            string fullName;
            if (fullNameCol.HasValue)
            {
                fullName = GetCellRawText(row.Cell(fullNameCol.Value));
            }
            else
            {
                var first = GetCellRawText(row.Cell(firstNameCol!.Value));
                var last = GetCellRawText(row.Cell(lastNameCol!.Value));
                fullName = $"{first} {last}".Trim();
            }

            var convenio = GetCellRawText(row.Cell(convenioCol.Value));
            var phone = GetCellRawText(row.Cell(phoneCol.Value));

            if (string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(convenio) && string.IsNullOrWhiteSpace(phone))
                continue;

            result.Add(new CustomerImportRowDto
            {
                RowNumber = rowNum,
                FullName = fullName,
                Convenio = convenio,
                PhoneNumber = phone
            });
        }

        return result;
    }

    private static (int? fullNameCol, int? firstNameCol, int? lastNameCol, int? convenioCol, int? phoneCol) MapColumns(IXLRow headerRow)
    {
        int? fullNameCol = null, firstNameCol = null, lastNameCol = null, convenioCol = null, phoneCol = null;

        foreach (var cell in headerRow.CellsUsed())
        {
            var normalized = Normalize(cell.GetString());
            var colNum = cell.Address.ColumnNumber;

            if (fullNameCol is null && FullNameAliases.Any(a => Normalize(a) == normalized))
                fullNameCol = colNum;
            else if (firstNameCol is null && FirstNameAliases.Any(a => Normalize(a) == normalized))
                firstNameCol = colNum;
            else if (lastNameCol is null && LastNameAliases.Any(a => Normalize(a) == normalized))
                lastNameCol = colNum;
            else if (convenioCol is null && ConvenioAliases.Any(a => Normalize(a) == normalized))
                convenioCol = colNum;
            else if (phoneCol is null && PhoneAliases.Any(a => Normalize(a) == normalized))
                phoneCol = colNum;
        }

        return (fullNameCol, firstNameCol, lastNameCol, convenioCol, phoneCol);
    }

    private static string Normalize(string input)
    {
        var trimmed = input.Trim().ToUpperInvariant();
        var decomposed = trimmed.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString();
    }

    private static string GetCellRawText(IXLCell cell)
    {
        if (cell.IsEmpty()) return string.Empty;

        if (cell.DataType == XLDataType.Number)
            return cell.GetValue<double>().ToString("F0", CultureInfo.InvariantCulture);

        return cell.GetString().Trim();
    }
}