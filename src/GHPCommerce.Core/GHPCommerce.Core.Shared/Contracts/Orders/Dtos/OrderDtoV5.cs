using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Dtos
{
    public class OrderDtoV5
    {
        public string CreatedBy { get; set; }
        public int OrderType { get; set; }
        public int SequenceNumber { get; set; }
        public string RefDocument { get; set; }
        public Guid CustomerId { get; set; }
        public Guid CreatedById { get; set; }
        public string CodeAx { get; set; }
        public int OrderStatus { get; set; }
    }
}