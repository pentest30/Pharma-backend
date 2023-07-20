using System;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Organizations.DTOs
{
    public class OrganizationDto : ICommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OrganizationGroupCode { get; set; }
    }
    public class OrganizationDtoConfigurationMapping : Profile
    {
        public OrganizationDtoConfigurationMapping()
        {
            CreateMap<OrganizationDto, Domain.Domain.Tiers.Organization>().ReverseMap();
        }
    }
}
