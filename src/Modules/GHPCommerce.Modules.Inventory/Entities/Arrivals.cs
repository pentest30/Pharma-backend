using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GHPCommerce.Modules.Inventory.Entities
{
    
    public class Arrivals
    {
        [Column("asCode_prod")]
        public string ProductCode { get; set; }
        [Column("Designation")]
        public string ProductName { get; set; }
        [Column("NumLOT")]
        public string InternalBatchNumber { get; set; }
        [Column("Site")]
        public string Site { get; set; }
        [Column("Qte")]
        public double Quantity { get; set; }
        [Column("DateLivraison")]
        public DateTime DeliveryDate { get; set; }
        [Column("PPA")]
        public double PPA { get; set; }
        [Column("DDP")]
        public DateTime ExpiryDate { get; set; }
        
    }
}