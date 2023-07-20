using AutoMapper;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;
using GHPCommerce.Modules.Procurement.Commands.Receipts;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;

namespace GHPCommerce.Modules.Procurement.MapperProfiles
{
    public class DeliveryReceiptMapping : Profile
    {
        public DeliveryReceiptMapping()
        {
            CreateMap<DeliveryReceipt, DeliveryReceiptDto>().ForMember(x=>x.DeliveryReceiptSequenceNumber, o=> o.MapFrom(o=> MapSequenceNumber(o))).ReverseMap();
            CreateMap<DeliveryReceiptItem, DeliveryReceiptItemDto>().ReverseMap();
            CreateMap<DeliveryReceipt, CreateReceiptItemCommand>()
                .ForMember(x=>x.DeliveryReceiptId, o => o.MapFrom(p=> p.Id)).ReverseMap();
            CreateMap<DeliveryReceiptItem, CreateReceiptItemCommand>().ReverseMap();
            CreateMap<DeliveryReceiptItem, DeliveryItem>().ReverseMap();
            CreateMap<IDeliveryReceiptSubmittedEvent, InventoryMessage>().ReverseMap();

        }

        private string MapSequenceNumber(DeliveryReceipt deliveryReceipt)
        {
             return "BR-"+ deliveryReceipt.InvoiceDate.Date.ToString("yy-MM-dd").Substring(0,2)
                          +"/" +"0000000000".Substring(0,10-deliveryReceipt.SequenceNumber.ToString().Length)+ deliveryReceipt.SequenceNumber;

        }
    }
}