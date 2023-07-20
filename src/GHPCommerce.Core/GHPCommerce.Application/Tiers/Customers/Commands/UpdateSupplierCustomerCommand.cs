using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Customers.Commands
{
    public class UpdateSupplierCustomerCommand :ICommand<ValidationResult>
    {
        public Guid OrganizationId { get; set; }
        public Guid Id { get; set; }
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
        public decimal LimitCredit { get; set; }
        /// <summary>
        /// écheance
        /// </summary>
        public int PaymentDeadline { get; set; }
        public CustomerState CustomerState { get; set; }
        public PaymentMode PaymentMode { get; set; }
    }
}
