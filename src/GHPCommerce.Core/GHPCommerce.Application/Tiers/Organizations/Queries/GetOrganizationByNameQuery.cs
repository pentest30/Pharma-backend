using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Organizations.Queries
{
    public class GetOrganizationByNameQuery :ICommand<OrganizationDto>
    {
        public string Name { get; set; }
    }
}
