using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Commands
{
    public class CreateTherapeuticClassCommand :ICommand<ValidationResult>
    {
        public CreateTherapeuticClassCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateTherapeuticClassCommandValidator : AbstractValidator<CreateTherapeuticClassCommand>
    {
        public CreateTherapeuticClassCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
