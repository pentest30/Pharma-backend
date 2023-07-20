using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using System.Collections.Generic;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetCustomerBySalesPersonDash : ICommand<SyncPagedResult<CustomerDtoV2>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }

    }
}
