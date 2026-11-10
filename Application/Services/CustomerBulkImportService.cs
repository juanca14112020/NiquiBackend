using System.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.DTOs.Customer;
using NiquiBackend.Application.Interfaces.Infrastructure;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Common;
using NiquiBackend.Infrastructure.Persistence.Generated;

namespace NiquiBackend.Application.Services;

// Restringido a SuperAdmin y Admin (el Developer NO usa este metodo, por decision de negocio).
public class CustomerBulkImportService : ICustomerBulkImportService
{
    private readonly IExcelReaderService _excelReader;
    private readonly IBulkInsertService _bulkInsert;
    private readonly IValidator<CustomerImportRowDto> _validator;
    private readonly NiquiDbContext _context;

    // Evita que dos cargas simultaneas (doble envio, reintento) pasen ambas el chequeo de duplicados.
    private static readonly SemaphoreSlim ImportLock = new(1, 1);

    public CustomerBulkImportService(
        IExcelReaderService excelReader,
        IBulkInsertService bulkInsert,
        IValidator<CustomerImportRowDto> validator,
        NiquiDbContext context)
    {
        _excelReader = excelReader;
        _bulkInsert = bulkInsert;
        _validator = validator;
        _context = context;
    }

    public async Task<CustomerBulkImportResultDto> ImportAsync(
        Stream file, Guid currentUserId, string currentUserRole)
    {
        await ImportLock.WaitAsync();
        try
        {
            return await ImportInternalAsync(file, currentUserId, currentUserRole);
        }
        finally
        {
            ImportLock.Release();
        }
    }

    private async Task<CustomerBulkImportResultDto> ImportInternalAsync(
        Stream file, Guid currentUserId, string currentUserRole)
    {
        var rows = _excelReader.ReadCustomers(file);
        var result = new CustomerBulkImportResultDto { TotalRows = rows.Count };

        var table = new DataTable();
        table.Columns.Add("CustomerID", typeof(Guid));
        table.Columns.Add("FullName", typeof(string));
        table.Columns.Add("Convenio", typeof(string));
        table.Columns.Add("PhoneNumber", typeof(string));
        table.Columns.Add("IsApproved", typeof(bool));
        table.Columns.Add("IsCalled", typeof(bool));
        table.Columns.Add("CreatedBySuperAdminID", typeof(Guid));
        table.Columns.Add("CreatedByAdminID", typeof(Guid));
        table.Columns.Add("CreatedAt", typeof(DateTime));

        // Telefonos que ya existen en la BD + los que ya se agregaron desde este mismo archivo.
        var existingPhones = new HashSet<string>(
            await _context.Customers.AsNoTracking().Select(c => c.PhoneNumber).ToListAsync());

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

            if (!existingPhones.Add(row.PhoneNumber))
            {
                result.Rejected++;
                result.Errors.Add($"Fila {row.RowNumber}: el teléfono {row.PhoneNumber} ya existe (duplicado), no se insertó.");
                continue;
            }

            var dr = table.NewRow();
            dr["CustomerID"] = Guid.NewGuid();
            dr["FullName"] = row.FullName;
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