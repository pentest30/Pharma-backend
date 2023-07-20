using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Dtos
{
    public class OrderDtoV4
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ZoneGroupName { get; set; }
        public string SectorName { get; set; }
        public string SectorCode { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public Guid OrderId { get; set; }

        public int OrderNumberSequence { get; set; }

    }
}