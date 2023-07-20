using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Quota
{
    public class UpdateQuotaCommand : ICommand<ValidationResult>
    {
        public Guid? OldSalesPersonId { get; set; }
        public Guid? NewSalesPersonId { get; set; }
        public Guid ProductId { get; set; }
    }
    public class UpdateQuotaCommandValidator : AbstractValidator<UpdateQuotaCommand>
    {
        public UpdateQuotaCommandValidator()
        {
            RuleFor(v => v.OldSalesPersonId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.NewSalesPersonId)
              .Must(x => x != Guid.Empty);

        }
    }
}
