using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Procurement.Entities
{
    public class SupplierInvoiceItem : Entity<Guid>
    {
        public SupplierInvoiceItem()
        {
            Color = "";
            Size = "";
            
        }
        public Guid InvoiceId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
    
        public DateTime? ExpiryDate { get; set; }
        public SupplierInvoice Invoice { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal TotalInlTax { get; set; }
        public decimal TotalExlTax { get; set; }
        public string PackagingCode { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        public int Packing { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        /// <summary>
        /// prix de vente
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// marge fournisseur
        /// </summary>
        public decimal WholesaleMargin { get; set; }

        /// <summary>
        /// marge pharmacien
        /// </summary>
        public decimal PharmacistMargin { get; set; }

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

        #region Info Achat
/// <summary>
/// prix d'achat non remisé
/// </summary>
        public decimal PurchaseUnitPrice { get; set; }
        /// <summary>
        /// prix achat remisé
        /// </summary>
        public decimal PurchasePriceIncDiscount { get; set; }
        /// <summary>
        /// taux de remise achat
        /// </summary>
        public double Discount { get; set; }


        #endregion
    }
}