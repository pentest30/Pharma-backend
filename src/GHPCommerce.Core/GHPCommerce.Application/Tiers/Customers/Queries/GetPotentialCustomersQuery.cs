using System.Collections.Generic;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetPotentialCustomersQuery:ICommand<IEnumerable<OrganizationDto>>
    {

    }
}
