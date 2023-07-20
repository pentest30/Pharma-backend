using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetPagedOrdersQuery :ICommand<SyncPagedResult<OrderDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        
        public Boolean? IsPsy { get; set; }
    }
}
