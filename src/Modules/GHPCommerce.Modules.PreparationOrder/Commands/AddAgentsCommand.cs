using GHPCommerce.Domain.Domain.Commands;
using FluentValidation.Results;
using System;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class AddAgentsCommand : ICommand<ValidationResult>
    {
        public Guid PreparationOrderId { get; set; }
        public Guid ExecutedById { get; set; }
        public string ExecutedByName { get; set; }
        public Guid VerifiedById { get; set; }
        public string VerifiedByName { get; set; }
        public Guid PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
    }
}
