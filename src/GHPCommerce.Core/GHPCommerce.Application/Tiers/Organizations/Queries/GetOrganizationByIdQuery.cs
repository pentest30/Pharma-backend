using System;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Organizations.Queries
{
    public class GetOrganizationByIdQuery :ICommand<OrganizationDtoV2>
    {
        public Guid Id { get; set; }
    }
}
