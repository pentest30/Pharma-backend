using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;
namespace GHPCommerce.Modules.Quota.Entities
{
    public class Quota :  AggregateRoot<Guid>
    {
        public Quota()
        {
            QuotaTransactions = new List<QuotaTransaction>();
        }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
       
        public int InitialQuantity { get; set; }
        public DateTime QuotaDate { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public List<QuotaTransaction> QuotaTransactions { get; set; }
        
    }
}
