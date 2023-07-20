using System;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class GetProductByIdQuery : ICommand<ProductDtoV2>
    {
        public Guid CatalogId { get; set; }
        public Guid Id { get; set; }
    }
}
