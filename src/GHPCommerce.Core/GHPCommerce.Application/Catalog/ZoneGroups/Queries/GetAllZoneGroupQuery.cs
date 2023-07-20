using GHPCommerce.Application.Catalog.ZoneGroups.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System.Collections.Generic;

namespace GHPCommerce.Application.Catalog.ZoneGroups.Queries
{
    public class GetAllZoneGroupQuery : ICommand<IEnumerable<ZoneGroupDto>>
    {
    }
}
