using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries
{
    public class GetCustomerOrganizationIdsQuery : ICommand<IEnumerable<Guid>>
    {
        public Guid SupplierId { get; set; }
    }
}
