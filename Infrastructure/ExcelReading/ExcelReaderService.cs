using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.Interfaces.Infrastructure;

namespace NiquiBackend.Infrastructure.ExcelReading;

public class ExcelReaderService : IExcelReaderService
{
    // Alias aceptados para cada campo. Se comparan ya normalizados (sin tildes, en mayusculas, sin espacios extra).
    private static readonly Dictionary<string, string[]> ColumnAliases = new()
    {
        ["FirstName"] = new[] { "NOMBRE", "NOMBRES", "PRIMER NOMBRE", "FIRSTNAME" },
        ["LastName"] = new[] { "APELLIDO", "APELLIDOS", "LASTNAME" },
        ["Convenio"] = new[] { "CONVENIO" },
        ["PhoneNumber"] = new[] { "TELEFONO", "TELEFONOS", "CELULAR", "NUMERO", "NUMERO CELULAR", "TEL", "PHONE", "PHONENUMBER" }
    };

    public List<CustomerImportRowDto> ReadCustomers(Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheet(1);

        var headerRow = ws.Row(1);
        var columnMap = MapColumns(headerRow);

        var required = new[] { "FirstName", "LastName", "Convenio", "PhoneNumber" };
        var missing = required.Where(r => !columnMap.ContainsKey(r)).ToList();

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"No se encontraron en el Excel las columnas: {string.Join(", ", missing)}. " +
                "Verifica que los encabezados existan (ej: NOMBRE, APELLIDO, CONVENIO, TELEFONO).");
        }

        var result = new List<CustomerImportRowDto>();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int rowNum = 2; rowNum <= lastRow; rowNum++)
        {
            var row = ws.Row(rowNum);
            if (row.IsEmpty()) continue;

            var firstName = GetCellRawText(row.Cell(columnMap["FirstName"]));
            var lastName = GetCellRawText(row.Cell(columnMap["LastName"]));
            var convenio = GetCellRawText(row.Cell(columnMap["Convenio"]));
            var phone = GetCellRawText(row.Cell(columnMap["PhoneNumber"]));

            // Fila completamente vacia en los campos que nos importan: se ignora silenciosamente
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName)
                && string.IsNullOrWhiteSpace(convenio) && string.IsNullOrWhiteSpace(phone))
                continue;

            result.Add(new CustomerImportRowDto
            {
                RowNumber = rowNum,
                FirstName = firstName,
                LastName = lastName,
                Convenio = convenio,
                PhoneNumber = phone
            });
        }

        return result;
    }

    private static Dictionary<string, int> MapColumns(IXLRow headerRow)
    {
        var columnMap = new Dictionary<string, int>();

        foreach (var cell in headerRow.CellsUsed())
        {
            var normalizedHeader = Normalize(cell.GetString());

            foreach (var (field, aliases) in ColumnAliases)
            {
                if (columnMap.ContainsKey(field)) continue;

                if (aliases.Any(alias => Normalize(alias) == normalizedHeader))
                {
                    columnMap[field] = cell.Address.ColumnNumber;
                    break;
                }
            }
        }

        return columnMap;
    }

    // Quita tildes, espacios de mas, y pasa a mayusculas para poder comparar
    // "Teléfono", "TELEFONO ", "telefono" etc. como si fueran lo mismo.
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

    // Evita que numeros grandes (celulares) salgan en notacion cientifica o con decimales.
    private static string GetCellRawText(IXLCell cell)
    {
        if (cell.IsEmpty()) return string.Empty;

        if (cell.DataType == XLDataType.Number)
            return cell.GetValue<double>().ToString("F0", CultureInfo.InvariantCulture);

        return cell.GetString().Trim();
    }
}
