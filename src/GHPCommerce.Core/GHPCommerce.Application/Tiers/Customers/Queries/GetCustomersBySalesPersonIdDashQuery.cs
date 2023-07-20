using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System.Collections.Generic;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetCustomersBySalesPersonIdDashQuery : ICommand<IEnumerable<CustomerDtoV2>>
    {
    }
}
