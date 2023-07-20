using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Cache
{
    public class AtomOrderContract
    {
        public AtomOrderContract()
        {
            Items = new List<AtomOrderItem>();
        }
         // code client 
        public string CustomerId { get; set; }
       // code filliale
        public string SupplierId { get; set; }
        public string RefDoc { get; set; }
        public bool Psychotropic { get; set; }
        public int OrderStatus { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalNetAmount { get; set; }
        public decimal? DiscTotValue { get; set; }
        public Guid OrderId { get; set; }
        public List<AtomOrderItem> Items { get; set; }
    }

    public class AtomOrderItem
    {
        public string BatchNumber { get; set; }
        public decimal? DiscValue { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? UnitPrice { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
    }
}