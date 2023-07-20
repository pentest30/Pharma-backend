using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.PreparationOrder.DTOs.DeleiveryOrders
{
    public class DeleiveryOrderDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid  deleiveryOrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid OrganizationId { get; set; }
        public string CustomerName { get; set; }
        public Guid SupplierId { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public int SequenceNumber { get; set; }

        public string DeleiveryOrderNumber => "BL-"+DeleiveryOrderDate.Date.ToString("yy-MM-dd").Substring(0,2)
                                                   +"/" +"0000000000".Substring(0,10-SequenceNumber.ToString().Length)+ SequenceNumber;
        public DateTime DeleiveryOrderDate { get; set; }
        public bool Validated { get; set; }
        public string CreatedBy{ get; set; }
        public string UpdatedBy { get; set; }
        public List<DeleiveryOrderItemDto> DeleiveryOrderItems { get; set; }
    }
}