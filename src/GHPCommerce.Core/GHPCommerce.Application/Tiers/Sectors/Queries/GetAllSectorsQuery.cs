using System.Collections.Generic;
using GHPCommerce.Application.Tiers.Sectors.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Sectors.Queries
{
    public class GetAllSectorsQuery :ICommand<IEnumerable<SectorDto>>
    {

    }
}
