using AutoMapper;
using GHPCommerce.Modules.Inventory.Commands.TransferLogs;
using GHPCommerce.Modules.Inventory.DTOs.TransferLogs;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.MapperProfiles
{
    public class TransferLogConfigMapping : Profile
    {
        public TransferLogConfigMapping()
        {
            CreateMap<TransferLog, TransferLogCreateCommand>().ReverseMap();
            CreateMap< TransferLog, TransferLogDto>()
                .ForMember(x=>x.TransferLogSequenceNumber, o => o.MapFrom(p=> GetSequenceNumber(p)))
                .ForMember(x=>x.TransferLogId, o => o.MapFrom(p=> p.Id))

                .ReverseMap();
            CreateMap<TransferLogItem, TransferLogCreateCommand>()
              
                .ReverseMap();
            CreateMap<TransferLogItem, TransferLogItemDto>() 
                .ForMember(x=>x.ZoneSourceName, o => o.MapFrom(p=> p.TransferLog!=null? p.TransferLog.ZoneSourceName : ""))
                .ForMember(x=>x.ZoneDestName, o => o.MapFrom(p=>p.TransferLog!=null? p.TransferLog.ZoneDestName : ""))
                .ForMember(x=>x.StockStateName, o => o.MapFrom(p=>p.TransferLog!=null? p.TransferLog.StockStateName : ""))
                .ReverseMap();
        }

        private object GetSequenceNumber(TransferLog transferLogDto)
        {
            return "TL-"+ transferLogDto.CreatedDateTime.Date.ToString("yy-MM-dd").Substring(0,2)
                        +"/" +"0000000000".Substring(0,10-transferLogDto.SequenceNumber.ToString().Length)+ transferLogDto.SequenceNumber;

        }
    }
}