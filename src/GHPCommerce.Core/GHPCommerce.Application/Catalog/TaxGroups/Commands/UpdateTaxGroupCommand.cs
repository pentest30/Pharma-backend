using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TaxGroups.Commands
{
    public class UpdateTaxGroupCommand :ICommand<ValidationResult>
    {
        public UpdateTaxGroupCommand()
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
    public class UpdateTaxGroupCommandValidator : AbstractValidator<UpdateTaxGroupCommand>
    {
        public UpdateTaxGroupCommandValidator()
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
                .Must((time => time != default));
        }
    }
}
