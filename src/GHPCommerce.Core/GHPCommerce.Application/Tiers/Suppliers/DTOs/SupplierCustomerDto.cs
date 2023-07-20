using System;
using System.Collections.Generic;
using GHPCommerce.Application.Tiers.BankAccounts.DTOs;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Suppliers.DTOs
{
    public class SupplierCustomerDto
    {
        public string Code { get; set; }
        public decimal Dept { get; set; }
        public Guid Id { get; set; }
        public bool? OnlineCustomer { get; set; }
        public bool? IsPickUpLocation { get; set; }
        public string DeliveryTypeDescription { get; set; }
        public DeliveryType DeliveryType { get; set; }
       
        public List<ProductClass> AllowedProductClasses { get; set; }
        public Guid? TaxGroupId { get; set; }
        public bool? QuotaEligibility { get; set; }
        public Guid? DefaultSalesPerson { get; set; }
        public Guid? DefaultSalesGroup { get; set; }
        public string DefaultSalesGroupName { get; set; }
        public string SalesPersonName { get; set; }

        public Guid? DefaultDeliverySector { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        public string OrganizationStatusDescription { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentMode PaymentMode { get; set; }

        public decimal LimitCredit { get; set; }
        public List<AddressDto> Addresses { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
        public List<BankAccountDto> BankAccounts { get; set; }
        public List<EmailDto> Emails { get; set; }
        public string OrganizationGroupCode { get; set; }
        public string NIS { get; set; }
        public string NIF { get; set; }
        public string RC { get; set; }
        public string AI { get; set; }
        public string DisabledReason { get; set; }
        public string Activity { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public string ECommerce { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedDateTime { get; set; }
        public int PaymentDeadline { get; set; }
        public CustomerState CustomerState { get; set; }
        public string Sector { get; set; }
        public decimal MonthlyObjective { get; set; }
        public decimal Debt { get; set; }
        public string SectorCode { get; set; }


    }

}
