using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.PickingZones.Commands
{
    public class DeletePickingZoneCommand  :ICommand
    {
        public Guid Id { get; set; }
    }
}
