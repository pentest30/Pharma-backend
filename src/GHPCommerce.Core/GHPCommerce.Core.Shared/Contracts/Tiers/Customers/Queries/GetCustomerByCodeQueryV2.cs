using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries
{
    public class GetCustomerByCodeQueryV2 : ICommand<CustomerDtoV1>
    {
        public string Code { get; set; }
        public Guid SupplierOrganizationId { get; set; }
    }
}