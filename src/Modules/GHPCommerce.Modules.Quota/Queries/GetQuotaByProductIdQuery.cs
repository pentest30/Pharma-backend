using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Quota.DTOs;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class GetQuotaByProductIdQuery : ICommand<QuotaDtoV1>
    {
        public Guid ProductId { get; set; }
        public DateTime Date { get; set; }
    }
}
