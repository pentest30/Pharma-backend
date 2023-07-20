using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Modules.PreparationOrder.Entities
{
    public class PreparationOrderVerifier : Entity<Guid>
    {
        public Guid PreparationOrderId { get; set; }
        public Guid VerifiedById { get; set; }
        public string VerifiedByName { get; set; }
        public DateTime VerifiedTime { get; set; }
        public Guid PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        //public PreparationOrder PreparationOrder { get; set; }

    }
}
