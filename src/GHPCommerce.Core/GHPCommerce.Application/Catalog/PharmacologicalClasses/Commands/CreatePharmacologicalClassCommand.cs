using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Commands
{
    public class CreatePharmacologicalClassCommand :ICommand<ValidationResult>
    {
        public CreatePharmacologicalClassCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreatePharmacologicalClassCommandValidator : AbstractValidator<CreatePharmacologicalClassCommand>
    {
        public CreatePharmacologicalClassCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
          
        }
    }
}
