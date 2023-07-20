using System.Collections.Generic;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Suppliers.Queries
{
    public class GetSuppliersForB2BCustomerListQuery : ICommand<IEnumerable<OrganizationDto>>
    {
    }
}
