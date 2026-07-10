using FluentValidation;
using NiquiBackend.Application.DTOs.Customer;

namespace NiquiBackend.Application.Validators;

public class CustomerImportRowValidator : AbstractValidator<CustomerImportRowDto>
{
    public CustomerImportRowValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(101);
        RuleFor(x => x.Convenio).NotEmpty().MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+573\d{9}$")
            .WithMessage("El número no se pudo reconocer como un celular colombiano válido.");
    }
}