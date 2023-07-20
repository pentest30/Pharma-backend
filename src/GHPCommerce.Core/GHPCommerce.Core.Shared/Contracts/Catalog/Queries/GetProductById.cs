using System;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetProductById :ICommand<ProductDtoV3>
    {
        public Guid Id { get; set; }
    }
}
