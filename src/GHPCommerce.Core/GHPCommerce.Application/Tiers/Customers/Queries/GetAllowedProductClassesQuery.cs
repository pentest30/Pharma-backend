using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetAllowedProductClassesQuery : ICommand<IEnumerable<AllowedProductClass>>
    {
        public Guid CustomerOrganizationId { get; set; }
        public Guid SupplierOrganizationId { get; set; }
    }
}
