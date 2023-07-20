using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries
{
    public class GetCustomerByCodeQuery : ICommand<CustomerDtoV1>
    {
        public string Code { get; set; }
    }
}