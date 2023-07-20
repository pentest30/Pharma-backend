using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Quota.DTOs;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class GetDetailedQuotaQuery : ICommand<IEnumerable<QuotaDto>>
    {
        public Guid ProductId { get; set; }
        public Guid SalesPersonId { get; set; }
    }
}