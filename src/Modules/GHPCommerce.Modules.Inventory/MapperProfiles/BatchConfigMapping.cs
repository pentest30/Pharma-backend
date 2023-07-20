using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Batches.Commands;
using GHPCommerce.Core.Shared.Contracts.Batches.Dtos;
using GHPCommerce.Modules.Inventory.DTOs.Batches;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.MapperProfiles
{
    public class BatchConfigMapping : Profile
    {
        public BatchConfigMapping()
        {
            CreateMap<Batch, CreateBatchCommand>().ReverseMap();
            CreateMap<Batch, BatchDto>()
                .ForMember(x=>x.ExpiryDateShort, o=> o.MapFrom(p=>p.ExpiryDate!=null?  p.ExpiryDate.Value.ToShortDateString() : ""))
                .ReverseMap();
            CreateMap<Batch  , BatchDtoV1>().ReverseMap();

        }
    }
}