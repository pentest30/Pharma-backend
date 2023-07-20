using AutoMapper;
using GHPCommerce.Application.Tiers.Guests.Commands;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Dtos;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Tiers.Guest;
using GHPCommerce.Modules.Sales.Commands;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Models;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.ShoppingCarts.Commands;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Modules.Sales.Commands.Orders;
using GHPCommerce.Modules.Sales.DTOs.FinancialTransactions;
using GHPCommerce.Modules.Sales.DTOs.Invoices;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using GHPCommerce.Modules.Sales.DTOs.CreditNotes;
using GHPCommerce.Core.Shared.Contracts.Customer.CreditNotes.DTOs;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;
using GHPCommerce.Core.Shared.Events.CreditNotes;
using GHPCommerce.Modules.Sales.DTOs.Discounts;

namespace GHPCommerce.Modules.Sales.MapperProfiles
{
    public class SalesConfigMapping : Profile
    {
        public SalesConfigMapping()
        {
            CreateMap<OrderModel, OrderItem>().ReverseMap();
            CreateMap<Order, CachedOrder>().ReverseMap();
            CreateMap<OrderCreateCommand, CachedOrder>()
                .ForMember(x => x.Id, m => m.MapFrom(o =>  o.OrderId))

                .ReverseMap();
            CreateMap<OrderItemCreateCommand, CachedOrderItem>().ReverseMap();

            CreateMap<Order, OrderDtoV3>().ReverseMap();
            CreateMap<Order, OrderDtoV4>().ReverseMap();

            CreateMap<OrderDtoV2, OrderDtoV3>().ReverseMap();
            CreateMap<OrderItemDto , OrderItemDtoV2>().ReverseMap();
            CreateMap<OrderItem , OrderItemDtoV2>().ReverseMap();
            CreateMap<OrderItem, CachedOrderItem>().ReverseMap();
            CreateMap<OrderItemModel, OrderItemCreateCommand>().ReverseMap();
            CreateMap< OrderItem, PreparationOrderItemDtoV1>().ReverseMap();
            CreateMap<OrderItemUpdateCommandV2, OrderItem>().ReverseMap();
            CreateMap<OrderItem, InventSumReservationDto>().ReverseMap();
            CreateMap<CachedOrderItem, InventSumReservationDto>().ReverseMap();
            CreateMap<CachedOrder, OrderDto>().ReverseMap();
            CreateMap<CachedOrderItem, CachedInventSum>()
                .ForMember(x => x.PhysicalReservedQuantity, o =>
                o.MapFrom(or => or.Quantity)).ForMember(x => x.SalesUnitPrice, o =>
                o.MapFrom(x => x.UnitPrice))
               .ReverseMap();

            CreateMap<OrderModel, CreateOrderByPharmacistCommand>().ReverseMap();
            CreateMap<OrderDto, Order>()    
                .ForMember(x => x.Comment, o =>
                o.MapFrom(or => or.ErrorMsg))    .ReverseMap();
            CreateMap<OrderDtoV2, Order>().ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<CreateShoppingCartItemCommand, ShoppingCartItem>().ReverseMap();
            CreateMap<ShoppingCartItemModel, ShoppingCartItem>().ReverseMap();
            CreateMap<ShoppingCartItemModel, ShoppingCartItemDto>().ReverseMap();
            CreateMap<CreateGuestPickupCommand, GuestPickupModel>().ReverseMap();
            CreateMap<CreateGuestShipCommand, GuestShipModel>().ReverseMap();
            CreateMap<OrderDtoV2, CachedOrder>().ReverseMap();

            CreateMap<CreateDiscountCommand, Discount>().ReverseMap();
            CreateMap<UpdateDiscountCommand, Discount>().ReverseMap();
            CreateMap<DiscountDto, Discount>().ReverseMap();
            CreateMap<DiscountDtoV1, Discount>().ReverseMap();

            CreateMap<Discount, DiscountDto>()
                .ForMember(x => x.fromDateShort, m => m.MapFrom(o =>  o.from.Date.ToShortDateString()))
                .ForMember(x => x.toDateShort, m => m.MapFrom(o => o.to.Date.ToShortDateString()))
                .ReverseMap();
            CreateMap<CreditNoteDto, CreditNote>().ReverseMap();
            CreateMap<CreditNoteItemDto, CreditNoteItem>().ReverseMap();

            CreateMap<InvoiceDto, Invoice>().ReverseMap();
            CreateMap<InvoiceDtoV1, Invoice>().ReverseMap();
            CreateMap<InvoiceItem, InvoiceItemDto>().ReverseMap();
            CreateMap<DeliveryOrderItemDtoV1, InvoiceItem>().ReverseMap();
            CreateMap<FinancialTransaction  , FinancialTransactionDto >().ReverseMap();
            CreateMap<CreditNoteItem, CreditNoteItemForEvent>().ReverseMap();
            CreateMap<ICreditNoteSubmittedEvent, CreditNoteInventoryMessage>().ReverseMap();
        }
    }
}