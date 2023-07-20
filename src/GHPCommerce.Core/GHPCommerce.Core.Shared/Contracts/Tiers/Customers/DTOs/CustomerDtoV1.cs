using AutoMapper;
using System;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs
{
    public class CustomerDtoV1
    {
    
        public Guid? CustomerId { get; set; }
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid SupplierId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Sector { get; set; }
        public string SectorCode { get; set; }        
        public string City { get; set; }        
        public string Street { get; set; }        
        public string ZipCode { get; set; }
        public int DeadLine { get; set; }
        public string SalesManager { get; set; }
        public Guid? SalesPersonId { get; set; }
        public Guid? ActualSalesPersonId { get; set; }
        public string DefaultSalesPersonName { get; set; }
        public string ActualSalesPersonName { get; set; }
        public CustomerState CustomerState { get; set; }
        public decimal Dept { get; set; }
        public decimal LimitCredit { get; set; }
        public decimal MonthlyObjective { get; set; }

    }
    public class OrganizationDtoConfigurationMapping : Profile
    {
        public OrganizationDtoConfigurationMapping()
        {
            CreateMap<CustomerDtoV1, Domain.Domain.Tiers.Organization>().ReverseMap();
        }
    }
}
