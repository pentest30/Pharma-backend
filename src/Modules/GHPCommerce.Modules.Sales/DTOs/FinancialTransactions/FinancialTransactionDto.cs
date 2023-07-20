using System;
using GHPCommerce.Modules.Sales.Entities.Billing;

namespace GHPCommerce.Modules.Sales.DTOs.FinancialTransactions
{
    public class FinancialTransactionDto
    {
        public Guid OrganizationId { get; set; }
        public string RefDocument { get; set; }
        public DateTime DocumentDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public FinancialTransactionType FinancialTransactionType { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset? UpdatedDateTime { get; set; }

        public Guid UpdatedByUserId { get; set; }
    }
}