using System.Collections.Generic;
using GHPCommerce.Application.Catalog.Lists.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Lists.Queries
{
    public class GetAllListQuery : ICommand<IEnumerable<ListDto>>
    {
    }
}
