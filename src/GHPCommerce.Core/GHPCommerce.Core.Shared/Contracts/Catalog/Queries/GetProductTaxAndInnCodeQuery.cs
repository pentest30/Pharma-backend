using System;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetProductTaxAndInnCodeQuery  : ICommand<ProductDtoV4>
    {
        public Guid ProductId { get; set; }
    }
}
