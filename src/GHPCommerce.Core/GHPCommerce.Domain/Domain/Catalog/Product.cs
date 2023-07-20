using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.ValueObjects;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class Product : AggregateRoot<Guid>
    {
        
        private Product()
        {
            Images = new List<ImageItem>();
        }

        public Product(string name)
        {
            FullName = name;
            ProductState = ProductState.Valid;

        }
        public string Code { get; set; }
        public string FullName { get; set; }
        [Column(TypeName = "text")]
        public string Description { get; set; }
        public string RegistrationNumber { get; set; }
        public decimal PublicPrice { get; set; }
        public decimal ReferencePrice { get; set; }
        public bool Imported { get; set; }
        public bool Refundable { get; set; }
        public bool Psychotropic { get; set; }
        public bool Thermolabile { get; set; }
        public bool Removed { get; set; }
        public bool ForHospital { get; set; }
        public bool Princeps { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PackagingContent { get; set; }
        public string PackagingContentUnit { get; set; }
        public string DciConcat { get; set; }
        public string DosageConcat { get; set; }
        public int Packaging { get; set; }
        public string DefaultLocation { get; set; }
        public  Guid? ProductClassId { get; set; }
        public Guid? TherapeuticClassId { get; set; }
        public Guid? PharmacologicalClassId { get; set; }
        public Guid? INNCodeId { get; set; }
        public Guid TaxGroupId { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? ManufacturerId { get; set; }
        public Guid? ListId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public TaxGroup TaxGroup { get; set; }
        public ProductClass ProductClass { get; set; }
        public TherapeuticClass TherapeuticClass { get; set; }
        public INNCode InnCode { get; set; }
        public Brand Brand { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public PharmacologicalClass PharmacologicalClass { get; set; }
        public PickingZone PickingZone { get; set; }
        public List List { get; set; }
        public ProductState ProductState { get; set; }
        public ICollection<ImageItem> Images { get; set; }
        public Guid? PackagingId { get; set; }
        public Packaging PackagingItem { get; set; }
        public string ExternalCode { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public string DciCode { get; set; }
        public string ProductGroup { get; set; }
        public bool Quota  { get; set; }
    }
}
