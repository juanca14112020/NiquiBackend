using FluentValidation;
using NiquiBackend.Application.DTOs.Customer;

namespace NiquiBackend.Application.Validators;

public class CustomerImportRowValidator : AbstractValidator<CustomerImportRowDto>
{
    public CustomerImportRowValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Convenio).NotEmpty().MaximumLength(100);

        // En este punto PhoneNumber ya deberia venir normalizado (+573XXXXXXXXX)
        // por el CustomerBulkImportService, antes de llegar aca.
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+573\d{9}$")
            .WithMessage("El número no se pudo reconocer como un celular colombiano válido.");
    }
}
