using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetPagedDiscountsQuery : ICommand<SyncPagedResult<DiscountDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
}
