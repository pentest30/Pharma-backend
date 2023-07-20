using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Dosages.Commands
{
    public class CreateDosageCommand : ICommand<ValidationResult>
    {
        public CreateDosageCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateDosageCommandValidator : AbstractValidator<CreateDosageCommand>
    {
        public CreateDosageCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
