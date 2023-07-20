using GHPCommerce.Domain.Domain.Commands;
using System;
using System.ComponentModel.DataAnnotations;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class UpdateQuotaSalesPersonCommand : ICommand<ValidationResult>
    {
        public Guid QuotaId { get; set; }
        public Guid DestSalesPerson { get; set; }
    }
}
