using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Events.DeliveryOrders
{
    public class DeliveryOrder
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
        public string DeleiveryOrderNumber => "BL-"+DeleiveryOrderDate.Date.ToString("yy-MM-dd").Substring(0,2)
                                                   +"/" +"0000000000".Substring(0,10-SequenceNumber.ToString().Length)+ SequenceNumber;
        public int SequenceNumber { get; set; }

        public string OrderIdentifier { get; set; }
        public string CodeAx { get; set; }
        public List<DeliveryOrderItem> DeleiveryOrderItems { get; set; }
    }
}