using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetActiveDiscountByProductQuery : ICommand<IEnumerable<DiscountDto>>
    {
        public Guid ProductId { get; set; }
        public Guid? OrganizationId { get; set; }
    }
}
