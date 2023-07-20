using System;
using GHPCommerce.Core.Shared.Contracts.ZoneGroup.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.ZoneGroup.Queries
{
    public class GetGroupZoneByIdQuery :ICommand<ZoneGroupDto>
    {
        public Guid Id { get; set; }
    }
}