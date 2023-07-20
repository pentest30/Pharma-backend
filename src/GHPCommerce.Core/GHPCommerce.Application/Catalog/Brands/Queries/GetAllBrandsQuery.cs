using System.Collections.Generic;
using GHPCommerce.Application.Catalog.Brands.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Brands.Queries
{
    public class GetAllBrandsQuery : ICommand<IEnumerable<BrandDto>>
    {
    }
}
