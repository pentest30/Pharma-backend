using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Organization.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Organization.Queries
{
    public class GetPharmacistQuery : ICommand<IEnumerable<PharmacistDto>>
    {
    }
}
