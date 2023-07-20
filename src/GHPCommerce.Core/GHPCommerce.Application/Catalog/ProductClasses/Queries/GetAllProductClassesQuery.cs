using System.Collections.Generic;
using GHPCommerce.Application.Catalog.ProductClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.ProductClasses.Queries
{
    public class GetAllProductClassesQuery : ICommand<IEnumerable<ProductClassDto>>{ }
}