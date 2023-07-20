using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Application.Catalog.ZoneGroups.Commands
{
    public class UpdateZoneGroupCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string Printer { get; set; }

    }
    public class UpdateZoneGroupCommandValidator : AbstractValidator<UpdateZoneGroupCommand>
    {
        public UpdateZoneGroupCommandValidator()
        {

        }

    }
}
