using System;

using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Sales.Entities.CreditNotes
{
    public class CreditNoteItem : Entity<Guid>
    {
        public CreditNoteItem()
        {
            CreatedDateTime = DateTimeOffset.Now;
        }

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
        public decimal PpaTTC  {get; set; }
        public decimal UnitPrice { get; set; }
        
        public decimal PurchaseDiscountUnitPrice { get; set; }
        public decimal  UnitPriceInclTax { get; set; }
        
        public double DiscountRate { get; set; }
        
        public double DisplayedDiscountRate { get; set; }

        public double Tax { get; set; }
        
        public decimal TotalTax { get; set; }
        
        public decimal TotalDiscount{ get; set; }
        
        public decimal DisplayedTotalDiscount { get; set; }
        public decimal TotalExlTax { get; set; }
        public decimal TotalInlTax { get; set; }
        public virtual CreditNote CreditNote { get;set; }
    }
}