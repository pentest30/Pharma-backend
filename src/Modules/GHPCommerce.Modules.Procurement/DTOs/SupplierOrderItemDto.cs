using System;

namespace GHPCommerce.Modules.Procurement.DTOs
{
    public class SupplierOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }      
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public DateTime? MinExpiryDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public int DaysInStock { get; set; }
        /// <summary>
        /// quantité facturée
        /// </summary>
        public int InvoicedQuantity { get; set; }

        /// <summary>
        /// quantité reçue
        /// </summary>
        public int ReceivedQuantity { get; set; }

        /// <summary>
        /// Quantité restante
        /// </summary>
        public int RemainingQuantity { get; set; }

        public decimal Tax { get; set; }
    }
}