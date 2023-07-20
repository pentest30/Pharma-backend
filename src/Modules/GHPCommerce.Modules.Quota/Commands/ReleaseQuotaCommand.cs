using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class ReleaseQuotaCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public Guid SalesPersonId { get; set; }
        
    }
}
