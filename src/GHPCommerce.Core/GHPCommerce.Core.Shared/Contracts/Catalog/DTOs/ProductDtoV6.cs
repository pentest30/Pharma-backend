using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.DTOs
{
    [Serializable]
    public class ProductDtoV6 : ICloneable
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public decimal SalePrice { get; set; }
        public bool Available => Quantity > 0;
        public Guid? ProductClassId { get; set; }
        public Guid? INNCodeId { get; set; }
        public float Discount { get; set; }
        public decimal Tax { get; set; }
        public string Manufacturer { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrganizationId { get; set; }
        public bool Psychotropic { get; set; }
        public int Quantity { get; set; }
        //Somme des Qté vendables libérées et non libérées
        public double TotalQnt { get; set; }
        // quantité RAL
        public double  TotalRQ { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}