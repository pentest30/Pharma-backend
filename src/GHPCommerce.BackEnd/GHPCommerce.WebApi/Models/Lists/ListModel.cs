using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.Lists.Commands;

namespace GHPCommerce.WebApi.Models.Lists
{
    public class ListModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal SHP { get; set; }
    }

    public class ListModelConfigurationMapping : Profile
    {
        public ListModelConfigurationMapping()
        {
            CreateMap<CreateListCommand, ListModel>().ReverseMap();
            CreateMap<UpdateLisCommand, ListModel>().ReverseMap();
        }
    }
}
