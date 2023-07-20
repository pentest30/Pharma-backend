using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.DTOs
{
    [Serializable]
    public class ProductDtoV5 : ICloneable
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public decimal SalePrice { get; set; }
        public decimal UnitPrice { get; set; }
        public IEnumerable<string> Images { get; set; }
        public Guid CatalogId { get; set; }
        public bool Available => Quantity > 0;
        public string ProductClassName { get; set; }
        public Guid? ProductClassId { get; set; }
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
        public string TaxGroup { get; set; }
        public bool Psychotropic { get; set; }
        public string INNCodeName { get; set; }
        public int Quantity { get; set; }
        public bool? HasQuota { get; set; }
        public Guid? PickingZoneId { get; set; }
        public bool Thermolabile { get; set; }
        public string DefaultLocation { get; set; }
        public decimal PFS { get; set; }
        //Somme des Qté vendables libérées et non libérées
        public double TotalQnt { get; set; }
        // quantité RAL
        public double  TotalRQ { get; set; }
        public string DciFullName { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}