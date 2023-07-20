using System.Collections.Generic;
using AutoMapper;
using GHPCommerce.Application.Catalog.Manufacturers.Commands;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.WebApi.Models.Manufacturers
{
    public class ManufacturerModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<Address> Addresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<EmailModel> Emails { get; set; }
    }

    public class ManufacturerModelConfigurationMapping : Profile
    {
        public ManufacturerModelConfigurationMapping()
        {
            CreateMap<CreateManufacturerCommand, ManufacturerModel>().ReverseMap();
            CreateMap<UpdateManufacturerCommand, ManufacturerModel>().ReverseMap();
            CreateMap<UpdateManufacturerByCodeCommand, ManufacturerModel>().ReverseMap();
        }
    }
}
