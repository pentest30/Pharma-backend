using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.ProductClasses.Commands
{
    public class UpdateProductClassCommand : ICommand<ValidationResult>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentProductClassId { get; set; }
        public bool IsMedicamentClass { get; set; }
        public string Code { get; set; }
        public Guid Id { get; set; }

    }

    public class UpdateProductClassModelValidator : AbstractValidator<UpdateProductClassCommand>
    {
        public UpdateProductClassModelValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}