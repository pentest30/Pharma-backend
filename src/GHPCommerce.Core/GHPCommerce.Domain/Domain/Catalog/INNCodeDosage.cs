using System;

namespace GHPCommerce.Domain.Domain.Catalog
{
    
    public class INNCodeDosage 
    { 
        public Guid INNCodeId { get; set; }
        public Guid INNId { get; set; }
        public Guid DosageId { get; set; }
        public INNCode InnCode { get; set; }
        public INN Inn { get; set; }
        public Dosage Dosage { get; set; }

        public INNCodeDosage()
        {
            
        }
        public INNCodeDosage(Guid innCodeId, Guid innId, Guid dosageId)
        {
            INNCodeId = innCodeId;
            DosageId = dosageId;
            INNId = innId;
        }
    }
}
