using System.Collections.Generic;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class GetQuotaProductsQuery :  ICommand<IEnumerable<ProductDto>>
    {
        
    }
}