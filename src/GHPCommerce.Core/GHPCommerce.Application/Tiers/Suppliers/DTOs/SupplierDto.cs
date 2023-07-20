using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Domain.Domain.Tiers;
using System;

namespace GHPCommerce.Application.Tiers.Suppliers.DTOs
{
    public class SupplierDto
    {
        public Guid EntityId { get; set; }
        public string Code { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? DefaultSalesPerson { get; set; }
        public string SalesGroup { get; set; }

        public AddressDto AddressDto { get; set; }
        public decimal Debt { get; set; }
        public int PaymentDeadline { get; set; }
        public ConventionType ConventionType { get; set; }
        public string PhoneNumber { get; set; }
        public string Sector { get; set; }
        public string SectorCode { get; set; }
        public CustomerState CustomerState { get; set; }
        public decimal LimitCredit { get; set; }
        public decimal MonthlyObjective { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public string PaymentMethod { get; set; }
        public string CustomerStatus { get; set; }
        public Guid OrganizationId { get; set; }
        public string CustomerGroup { get; set; }
        public string OrganizationGroupCode { get; set; }
        public string NIS { get; set; }
        public string NIF { get; set; }
        public string RC { get; set; }
        public string AI { get; set; }
        public OrganizationActivity Activity { get; set; }
        public string EstablishmentDateShort { get; set; }



    }
}
