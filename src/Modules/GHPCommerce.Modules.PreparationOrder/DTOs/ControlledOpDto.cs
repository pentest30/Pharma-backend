using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.PreparationOrder.DTOs
{
    public class ControlledOpDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string GroupZoneName { get; set; }
        public string OrderNumber { get; set; }

        public string OrderIdentifier { get; set; }
        public DateTime? OrderDate { get; set; }
        public Guid OrderId { get; set; }
        public List<PreparationOrderDtoV4> items { get; set; }

    }
}