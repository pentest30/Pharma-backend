using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Lists.Commands
{
    public class CreateListCommand  :ICommand<ValidationResult>
    {
        public CreateListCommand()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal SHP { get; set; }
    }

    public class CreateListCommandValidator : AbstractValidator<CreateListCommand>
    {
        public CreateListCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.Code)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.SHP)
                .GreaterThanOrEqualTo(0);
        }
    }
}
