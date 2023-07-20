using System.Collections.Generic;
using GHPCommerce.Application.Catalog.Packaging.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Packaging.Queries
{
    public class GetAllPackagingQuery : ICommand<IEnumerable<PackagingDto>>
    {
    }
}
