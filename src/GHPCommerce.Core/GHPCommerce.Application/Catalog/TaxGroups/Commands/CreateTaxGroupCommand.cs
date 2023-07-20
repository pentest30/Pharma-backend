using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TaxGroups.Commands
{
    public class CreateTaxGroupCommand : ICommand<ValidationResult>
    {
        public CreateTaxGroupCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal TaxValue { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class CreateTaxGroupCommandValidator : AbstractValidator<CreateTaxGroupCommand>
    {
        public CreateTaxGroupCommandValidator()
        {

            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.Code)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.TaxValue)
                .GreaterThanOrEqualTo(0);
            RuleFor(v => v.ValidFrom)
                .Must(( (time) => time != default));
        }
    }
}
