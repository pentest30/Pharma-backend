using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Quota.Entities
{
    public class QuotaRequest: AggregateRoot<Guid>
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public bool ForSuperVisor { get; set; }
        public bool ForBuyer { get; set; }
        public Guid? DestSalesPersonId { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public QuotaRequestStatus Status { get; set; }
    }
}