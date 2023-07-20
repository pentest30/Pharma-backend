using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Organizations.Queries
{
    public class PagedOrganizationsQuery : ICommand<SyncPagedResult<OrganizationDtoV1>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
}
