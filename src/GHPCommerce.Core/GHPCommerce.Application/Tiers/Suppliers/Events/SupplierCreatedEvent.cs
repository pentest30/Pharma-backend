using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Events;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Suppliers.Events
{
    public class SupplierCreatedEvent : IEvent
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid SupplierId { get; set; }
        public bool? OnlineCustomer { get; set; }
        public bool? IsPickUpLocation { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public List<Guid> AllowedProductClasses { get; set; }
        public Guid? TaxGroupId { get; set; }
        public bool? QuotaEligibility { get; set; }
        public Guid? DefaultSalesPerson { get; set; }
        public Guid? DefaultSalesGroup { get; set; }
        public Guid? DefaultDeliverySector { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        public decimal Dept { get; set; }
        public ConventionType ConventionType { get; set; }
        public int PaymentDeadline { get; set; }
        public decimal LimitCredit { get; set; }
        public string Code { get; set; }
        public bool IsSupplier { get; set; }
    }
}
