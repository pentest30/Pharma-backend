using System;
using AutoMapper;
using GHPCommerce.Domain.Domain.Events;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Organizations.Events
{
    public class OrganizationCustomerCreatedEvent : IEvent
    {
        public string Code { get; set; }
        public Guid OrganizationId { get; set; }
        public bool? OnlineCustomer { get; set; }
        public bool? IsPickUpLocation { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public bool? QuotaEligibility { get; set; }
        public string DefaultSalesPerson { get; set; }
        public string DefaultSalesGroup { get; set; }
        public string DefaultDeliverySector { get; set; }
        public decimal Dept { get; set; }
        public decimal LimitCredit { get; set; }

        public ConventionType ConventionType { get; set; }
        public int PaymentDeadline { get; set; }
        public CustomerState CustomerState { get; set; }
        public string CustomerGroup { get; set; }
        public decimal MonthlyObjective { get; set; }
        public string PaymentMethod { get; set; }
        public string OrganizationGroupCode { get; set; }
    }
    public class OrganizationSupplierConfigMapping : Profile
    {
        public OrganizationSupplierConfigMapping()
        {
            CreateMap<OrganizationCustomerCreatedEvent, SupplierCustomer>()
                .ForMember(x=>x.PaymentMode, o => o.MapFrom(p=> ConvertToPaymentEnum( p.PaymentMethod)))
                .ForMember(x=>x.DefaultSalesPerson, o=> o.Ignore())
                .ForMember(x => x.DefaultSalesGroup, o => o.Ignore()).
                ForMember(x => x.DefaultDeliverySector, o => o.Ignore()).ReverseMap();
        }

        private PaymentMode ConvertToPaymentEnum(string paymentMethod)
        {
            switch (paymentMethod)
            {
                case "Chèque" : return PaymentMode.Check;
                case "Virement" : return PaymentMode.Payment;
                case "Versement" : return PaymentMode.Payment;
                case "Carte de crédit" : return PaymentMode.CreditCard;
                case "Transfert" : return PaymentMode.Transfer;
                case "Traite" : return PaymentMode.Bill;
                case "Espèce" : return PaymentMode.Cash;
            }

            return PaymentMode.Check;
        }
    }
}
