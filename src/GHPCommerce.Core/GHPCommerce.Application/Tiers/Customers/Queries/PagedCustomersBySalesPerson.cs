using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class PagedCustomersBySalesPerson : ICommand<SyncPagedResult<CustomerDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
}
