using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class GetProductListForSalesPersonQuery : ICommand<IEnumerable<ProductDtoV5>>
    {
        public Guid? SalesPersonId { get; set; }
        public string SearchBy { get; set; }
        public bool? IsPsy { get; set; }
    }
}
