using System;
using GHPCommerce.Application.Catalog.Brands.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Brands.Queries
{
    public class GetBrandByIdQuery :ICommand<BrandDto>
    {
        public Guid Id { get; set; }

    }
}
