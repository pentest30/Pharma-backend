using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Sales.Entities.Billing
{
    public class FinancialTransaction  : AggregateRoot<Guid>
    {
        public Guid OrganizationId { get; set; }
        public string RefDocument { get; set; }
        public int RefNumber { get; set; }

        public string CustomerName { get; set; }
        
        public Guid CustomerId { get; set; }

        public Guid SupplierId { get; set; }
        
        public string SupplierName { get; set; }

        public DateTime DocumentDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public FinancialTransactionType FinancialTransactionType { get; set; }

    }
}