using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Packaging.Commands
{
    public class UpdatePackagingCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class UpdatePackagingCommandValidator : AbstractValidator<UpdatePackagingCommand>
    {
        public UpdatePackagingCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.Code)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
