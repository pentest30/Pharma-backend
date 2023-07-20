using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries
{
    public class GetCustomerByCodeQueryV3 : ICommand<CustomerDtoV1>
    {
        public string OrganizationCode { get; set; }
        public string CustomerCode { get; set; }
    }
}