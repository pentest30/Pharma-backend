using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Quota.Commands.QuotaRequests
{
    public class ValidateQuotaRequestCommandV3 : ICommand
    {
        public Guid Id { get; set; }
        public List<CreateQuotaCommand> Quotas { get; set; }
    }
}
