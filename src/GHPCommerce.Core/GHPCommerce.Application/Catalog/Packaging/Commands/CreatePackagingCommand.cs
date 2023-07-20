using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Packaging.Commands
{
    public class CreatePackagingCommand : ICommand<ValidationResult>
    {
        public CreatePackagingCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class CreatePackagingCommandValidator : AbstractValidator<CreatePackagingCommand>
    {
        public CreatePackagingCommandValidator()
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
