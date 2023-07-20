using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Queries
{
    public class GetOrdersByStatusQuery : ICommand<SyncPagedResult<OrderDtoV4>> 
    { 
        public SyncDataGridQuery DataGridQuery { get; set; }
        public int Status { get; set; }

    }
}
