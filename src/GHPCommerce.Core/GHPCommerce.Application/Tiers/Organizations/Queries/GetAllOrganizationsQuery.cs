using System.Collections.Generic;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Organizations.Queries
{
    public class GetAllOrganizationsQuery:ICommand<IEnumerable<OrganizationDto>>
    {
    }
}
