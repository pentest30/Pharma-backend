using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Application.Catalog.ZoneGroups.Commands
{
    public class DeleteZoneGroupCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
