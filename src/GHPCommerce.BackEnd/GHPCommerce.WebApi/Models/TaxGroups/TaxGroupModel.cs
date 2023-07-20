using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.TaxGroups.Commands;

namespace GHPCommerce.WebApi.Models.TaxGroups
{
    public class TaxGroupModel
    {
      
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal TaxValue { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class TaxGroupModelMappingConfiguration : Profile
    {
        public TaxGroupModelMappingConfiguration()
        {
            CreateMap<CreateTaxGroupCommand, TaxGroupModel>().ReverseMap();
            CreateMap<UpdateTaxGroupCommand, TaxGroupModel>().ReverseMap();
        }
    }
}
