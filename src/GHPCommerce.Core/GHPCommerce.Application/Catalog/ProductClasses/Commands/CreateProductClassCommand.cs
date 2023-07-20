using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.ProductClasses.Commands
{
    public class CreateProductClassCommand : ICommand<ValidationResult>
    {
        public CreateProductClassCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentProductClassId { get; set; }
        public bool IsMedicamentClass { get; set; }
        public string Code { get; set; }
    }
    public class CreateProductClassModelValidator : AbstractValidator<CreateProductClassCommand>
    {
        public CreateProductClassModelValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
