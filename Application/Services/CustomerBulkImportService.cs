using System.Data;
using FluentValidation;
using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.Interfaces.Infrastructure;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Common;

namespace NiquiBackend.Application.Services;

// Restringido a SuperAdmin y Admin (el Developer NO usa este metodo, por decision de negocio).
public class CustomerBulkImportService : ICustomerBulkImportService
{
    private readonly IExcelReaderService _excelReader;
    private readonly IBulkInsertService _bulkInsert;
    private readonly IValidator<CustomerImportRowDto> _validator;

    public CustomerBulkImportService(
        IExcelReaderService excelReader,
        IBulkInsertService bulkInsert,
        IValidator<CustomerImportRowDto> validator)
    {
        _excelReader = excelReader;
        _bulkInsert = bulkInsert;
        _validator = validator;
    }

    public async Task<CustomerBulkImportResultDto> ImportAsync(
        Stream file, Guid currentUserId, string currentUserRole)
    {
        var rows = _excelReader.ReadCustomers(file);
        var result = new CustomerBulkImportResultDto { TotalRows = rows.Count };

        var table = new DataTable();
        table.Columns.Add("CustomerID", typeof(Guid));
        table.Columns.Add("FirstName", typeof(string));
        table.Columns.Add("LastName", typeof(string));
        table.Columns.Add("Convenio", typeof(string));
        table.Columns.Add("PhoneNumber", typeof(string));
        table.Columns.Add("IsApproved", typeof(bool));
        table.Columns.Add("IsCalled", typeof(bool));
        table.Columns.Add("CreatedBySuperAdminID", typeof(Guid));
        table.Columns.Add("CreatedByAdminID", typeof(Guid));
        table.Columns.Add("CreatedAt", typeof(DateTime));

        foreach (var row in rows)
        {
            // Normaliza el telefono a +573XXXXXXXXX (agrega +57 si no lo tenia).
            // Si no se puede reconocer como celular colombiano, se deja el valor original
            // para que el mensaje de error del validador muestre lo que vino en el Excel.
            var normalizedPhone = ColombianPhoneNormalizer.Normalize(row.PhoneNumber);
            if (normalizedPhone != null)
                row.PhoneNumber = normalizedPhone;

            var validation = await _validator.ValidateAsync(row);
            if (!validation.IsValid)
            {
                result.Rejected++;
                result.Errors.Add(
                    $"Fila {row.RowNumber}: {string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))}");
                continue;
            }

            var dr = table.NewRow();
            dr["CustomerID"] = Guid.NewGuid();
            dr["FirstName"] = row.FirstName;
            dr["LastName"] = row.LastName;
            dr["Convenio"] = row.Convenio;
            dr["PhoneNumber"] = row.PhoneNumber;
            dr["IsApproved"] = false;
            dr["IsCalled"] = false;
            dr["CreatedBySuperAdminID"] =
                currentUserRole == "SuperAdmin" ? currentUserId : DBNull.Value;
            dr["CreatedByAdminID"] =
                currentUserRole == "Admin" ? currentUserId : DBNull.Value;
            dr["CreatedAt"] = DateTime.UtcNow;

            table.Rows.Add(dr);
        }

        if (table.Rows.Count > 0)
        {
            await _bulkInsert.BulkInsertAsync(table, "Customer");
            result.Inserted = table.Rows.Count;
        }

        return result;
    }
}
