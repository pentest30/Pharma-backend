using System;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Products.Queries
{

    /// <summary>
    /// this query returns a list of permitted products for b2b customer by vendor 
    /// </summary>
    public class GetProductListForB2BCustomerQuery : ICommand<PagingResult<ProductDtoV3>>
    {
        public Guid SupplierId { get; set; }
        public string Term { get; set; }
        public Guid? ProductId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }

    }
}
