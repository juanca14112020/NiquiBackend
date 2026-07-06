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
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^3\d\d{9}$")
            .WithMessage("El número debe ser un celilar colombiano válido (10 digitos, inicia en 3).");
    }
}