using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Common;
using Microsoft.EntityFrameworkCore.Internal;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public class SupplierCustomer  :Entity<Guid>
    {
        public string Code { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? TaxGroupId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }
        public bool OnlineCustomer { get; set; }
        public bool IsPickUpLocation { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public List<AllowedProductClass> PermittedProductClasses { get; set; }
        [ForeignKey("TaxGroupId")]
        public TaxGroup TaxGroup { get; set; }
        public SectorCustomer Sector { get; set; }
        public bool QuotaEligibility  { get; set; }
        public Guid? DefaultSalesPerson { get; set; }
        public Guid? ActualSalesPerson { get; set; }
        public Guid? DefaultSalesGroup { get; set; }
        public Guid? DefaultDeliverySector { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        /// <summary>
        /// créance
        /// </summary>
        public decimal Dept { get; set; }
        public decimal LimitCredit { get; set; }
        public ConventionType ConventionType { get; set; }
        /// <summary>
        /// écheance
        /// </summary>
        public int PaymentDeadline { get; set; }

        public CustomerState CustomerState { get; set; }
        public string CustomerGroup { get; set; }
        public decimal MonthlyObjective { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public string PaymentMethod { get; set; }
        public string SalesGroup { get; set; }
        public string SalesPersonName { get; set; }
        public string ActualSalesPersonName { get; set; }
        public void UpdateSupplierCustomer(Guid? taxGroupId, 
            bool? onlineCustomer, 
            bool? isPickUpLocation,
            DeliveryType deliveryType, 
            bool? quotaEligibility,
            Guid? defaultSalesPerson,
            Guid? defaultSalesGroup,
            Guid? defaultDeliverySector,
            OrganizationStatus organizationStatus, 
            CustomerState customerState,
            List<Guid> permittedProductClasses,
            PaymentMode paymentMode,
            int paymentDeadline,
            decimal limitCredit
            )
        {
            CustomerState = customerState;
            TaxGroupId = taxGroupId;
            if (onlineCustomer != null)
                OnlineCustomer = onlineCustomer.Value;
            if (isPickUpLocation != null)
                IsPickUpLocation = isPickUpLocation.Value;
            DeliveryType = deliveryType;
            if (quotaEligibility != null) QuotaEligibility = quotaEligibility.Value;
            DefaultSalesPerson = defaultSalesPerson;
            DefaultSalesGroup = defaultSalesGroup;
            DefaultDeliverySector = defaultDeliverySector;
            OrganizationStatus = organizationStatus;
            PermittedProductClasses.Clear();
            PaymentMode = paymentMode;
            PaymentDeadline = paymentDeadline;
            LimitCredit = limitCredit;
            if (permittedProductClasses != null && permittedProductClasses.Any())
            {
                foreach (var productClass in permittedProductClasses)
                    PermittedProductClasses.Add(new AllowedProductClass { ProductClassId =productClass, SupplierCustomerId = Id });
            }
        }

    }

    public enum PaymentMode
    {
        Check,
        Transfer,
        CreditCard,
        Payment,
        Cash,
        Bill
    }
}
