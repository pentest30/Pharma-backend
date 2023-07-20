using System;
using System.Collections.Generic;
using GHPCommerce.Application.Catalog.ZoneGroups.DTOs;
using GHPCommerce.Domain.Domain.Catalog;

namespace GHPCommerce.Application.Catalog.Products.DTOs
{
    public class ProductDtoV2
    {
        public string Code { get; set; }
        public string FullName { get; set; }

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
        public Guid? ProductClassId { get; set; }
        public string ProductClassName { get; set; }

        public Guid? TherapeuticClassId { get; set; }
        public string TherapeuticClassName { get; set; }

        public Guid? PharmacologicalClassId { get; set; }
        public string PharmacologicalClassName { get; set; }
        public Guid? INNCodeId { get; set; }
        public Guid TaxGroupId { get; set; }
        public Guid? BrandId { get; set; }
        public string BrandName { get; set; }

        public Guid ManufacturerId { get; set; }
        public Guid? ListId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public Guid Id { get; set; }
        public ProductState ProductState { get; set; }
        public decimal SHP { get; set; }
        public Guid? PackagingId { get; set; }
        public ICollection<string> Images { get; set; }
        public string  Manufacturer { get; set; }
        public bool QuantityAvailability { get; set; }
        public bool HasQuota { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public PickingZone PickingZone { get; set; }
        public ZoneGroupDto ZoneGroup { get; set; }
        


    }
   
}
