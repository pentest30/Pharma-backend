using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Quota.DTOs;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class GetPagedQuotasByProductQuery: ICommand<SyncPagedResult<QuotaDto>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}