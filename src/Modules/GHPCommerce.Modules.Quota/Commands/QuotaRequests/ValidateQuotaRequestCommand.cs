using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Quota.Commands.QuotaRequests
{
    public class ValidateQuotaRequestCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }
}