using System;
using AutoMapper;
using GHPCommerce.Application.Catalog.Packaging.Commands;

namespace GHPCommerce.WebApi.Models.Packaging
{
    public class PackagingModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class PackagingModelConfigurationMapping : Profile
    {
        public PackagingModelConfigurationMapping()
        {
            CreateMap<CreatePackagingCommand, PackagingModel>().ReverseMap();
            CreateMap<UpdatePackagingCommand, PackagingModel>().ReverseMap();
        }
    }
}
