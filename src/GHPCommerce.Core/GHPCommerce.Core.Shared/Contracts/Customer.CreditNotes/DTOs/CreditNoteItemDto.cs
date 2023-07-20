using System;

namespace GHPCommerce.Modules.Sales.DTOs.CreditNotes
{
    public class CreditNoteItemDto
    {
        public Guid Id { get; set; }
        public Guid? CreditNoteId { get; set; }

        public int LineNum { get; set; }

        public Guid ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string VendorBatchNumber { get; set; }

        public string InternalBatchNumber { get; set; }

        public DateTime ExpiryDate { get; set; }

        public int Quantity { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal UnitPrice { get; set; }
 

    }
}