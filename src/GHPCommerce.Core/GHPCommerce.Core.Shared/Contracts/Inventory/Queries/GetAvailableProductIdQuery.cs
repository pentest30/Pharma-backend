using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Queries
{
    public class GetAvailableProductIdQuery : ICommand<List<Guid>>
    {
        public Guid OrganizationId { get; set; }

    }
}