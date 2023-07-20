using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Modules.PreparationOrder.Entities
{
    public class ConsolidationOrder : AggregateRoot<Guid>
    {
        public Guid CustomerId { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string CustomerName { get; set; }
        public bool ReceptionExpedition { get; set; }
        public bool SentForExpedition { get; set; }

        public bool Printed { get; set; }
        public int PrintCount { get; set; }
        public Guid? PrintedBy { get; set; }
        public string PrintedByName { get; set; }
        public DateTime? PrintedTime { get; set; }
        public Guid OrderId { get; set; }
        public DateTime? OrderDate { get; set; }

        public string OrderIdentifier { get; set; }
        public Guid? ConsolidatedById { get; set; }
        public DateTime? ConsolidatedTime { get; set; }
        public string ConsolidatedByName { get; set; }
        public string EmployeeCode { get; set; }
        public string ReceivedInShippingBy { get; set; }
        public Guid? ReceivedInShippingId { get; set; }
        public DateTime? ReceptionExpeditionTime { get; set; }

        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public bool Consolidated { get; set; }

    }
}
