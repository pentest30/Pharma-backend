using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Modules.PreparationOrder.Entities
{
    public class PreparationOrderExecuter : Entity<Guid>
    {
        public Guid PreparationOrderId { get; set; }
        public Guid ExecutedById { get; set; }
        public string ExecutedByName { get; set; }
        public DateTime? ExecutedTime { get; set; }
        public Guid PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        //public PreparationOrder PreparationOrder { get; set; }


    }
   
}
