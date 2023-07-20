using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Dtos
{
    public class DeliveryOrderDtoV2
    {
        public Guid Id { get; set; }      

        public Guid OrderId { get; set; }
        public DateTime DeleiveryOrderDate { get; set; }
        public DateTime OrderDate { get; set; }

        public Guid CustomerId { get; set; }
        public Guid OrganizationId { get; set; }

        public string CustomerName { get; set; }

        public Guid SupplierId { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public int DeleiveryOrderNumberSequence { get; set; }


        public List<DeliveryOrderItemDtoV1> DeleiveryOrderItems { get; set; }
    }
}