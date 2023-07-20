using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.INNCodes.Commands
{
    public class CreateInnCodeCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid FormId { get; set; }
        public List<InnCodeDosageDto> InnCodeDosages { get; set; }

    }

    public class CreateInnCodeCommandValidator : AbstractValidator<CreateInnCodeCommand>
    {
        public CreateInnCodeCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.FormId)
                .NotEmpty().Must(v=> v != default);

            RuleFor(v => v.InnCodeDosages.Count)
                .GreaterThan(0);
        }
    }
}
