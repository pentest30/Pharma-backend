using GHPCommerce.Domain.Domain.Catalog;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.DTOs
{
    [Serializable]
    public class ProductDtoV3
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public decimal SalePrice { get; set; }
        public decimal UnitPrice { get; set; }
        public IEnumerable<string> Images { get; set; }
        public Guid CatalogId { get; set; }
        public bool Available { get; set; }
        public string ProductClassName { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid? INNCodeId { get; set; }
        public float Discount { get; set; }
        public decimal Tax { get; set; }
        public string Manufacturer { get; set; }
        public string Brand { get; set; }

        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public Guid ProductId { get; set; }

        public Guid OrganizationId { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? ManufacturerId { get; set; }
        public string TaxGroupCode { get; set; }
        public bool HasQuota { get; set; }
        public ProductState ProductState { get; set; }
        public bool Psychotropic { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string DefaultLocation { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public string PickingZoneName { get; set; }
        public string ZoneGroupName { get; set; }
        public int PickingZoneOrder { get; set; }
    }
}
