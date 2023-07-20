using System;
using AutoMapper;
using GHPCommerce.Modules.Quota.Commands;
using GHPCommerce.Modules.Quota.Commands.QuotaRequests;
using GHPCommerce.Modules.Quota.DTOs;
using GHPCommerce.Modules.Quota.Entities;

namespace GHPCommerce.Modules.Quota.MapperProfiles
{
    public class QuotaProfile : Profile
    {
        public QuotaProfile()
        {
            CreateMap<CreateQuotaCommand, Entities.Quota>()
                .ForMember(x=>x.AvailableQuantity , o=> o.MapFrom(p=>p.InitialQuantity))
                .ReverseMap();
            CreateMap< Entities.Quota, QuotaDto>()
                .ForMember(x=>x.QuotaDateShort , o=> o.MapFrom(p=>p.QuotaDate.ToShortDateString()))
                .ReverseMap();
            CreateMap<QuotaRequest, QuotaRequestDto>()
                .ForMember(x => x.DateShort, o => o.MapFrom(p => p.Date.ToShortDateString()))
                .ForMember(x => x.Status, o => o.MapFrom(p => GetStatus(p.Status)))
                    .ReverseMap();
            CreateMap<QuotaRequest, ReceivedQuotaDto>()
                .ForMember(x => x.DateShort, o => o.MapFrom(p => p.Date.ToShortDateString()))
                .ForMember(x => x.Status, o => o.MapFrom(p => GetStatus(p.Status)))

                .ReverseMap();
            CreateMap<CreateQuotaRequestCommand, QuotaRequest>()
                .ReverseMap();
            CreateMap<CreateQuotaInitStateCommand, QuotaInitState>().ReverseMap();

        }

        private string GetStatus(QuotaRequestStatus pStatus)
        {
            switch (pStatus)
            {
                case QuotaRequestStatus.Rejected: return "Demande rejetée";
                case QuotaRequestStatus.Validate: return "Demande validée";
                case QuotaRequestStatus.Wait: return "Demande en attente";

            }
            return String.Empty;
        }
    }
}