using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Quota.DTOs;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class GetQuotasByProductQuery : ICommand<IEnumerable<QuotaDto>>
    {
        public Guid ProductId { get; set; }
        public Guid? SalesPeronId { get; set; }
    }
}