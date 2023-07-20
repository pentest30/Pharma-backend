using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Quota.Commands.QuotaRequests
{
    public class RejectQuotaRequestCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
