using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Quota.Entities;

namespace GHPCommerce.Modules.Quota.DTOs
{
    public class QuotaDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public int InitialQuantity { get; set; }
        public DateTime QuotaDate { get; set; }
        public string QuotaDateShort { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int OldAvailableQuantity { get; set; }
        public string SalesPersonName { get; set; }
        public Guid? SalesPersonId { get; set; }
        public int TotalProduct { get; set; }
        public List<QuotaTransaction> QuotaTransactions { get; set; }
       
    }
}