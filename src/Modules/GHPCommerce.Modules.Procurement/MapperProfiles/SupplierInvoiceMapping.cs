using System;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Batches.Commands;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Transactions;
using GHPCommerce.Modules.Procurement.Commands.Invoices;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;

namespace GHPCommerce.Modules.Procurement.MapperProfiles
{
    public class SupplierInvoiceMapping : Profile
    {
        public SupplierInvoiceMapping()
        {
            CreateMap<SupplierInvoice, SupplierInvoiceDto>()
                .ForMember(x=>x.InvoiceSequenceNumber, o=> o.MapFrom(i=> i.InvoiceSequenceNumber))
                .ForMember(x=>x.Status, o=> o.MapFrom(i=> GetInvoiceStatus((int)i.InvoiceStatus)))

                .ReverseMap();
            CreateMap<SupplierInvoiceItem, SupplierInvoiceItemDto>().ReverseMap();
            CreateMap<SupplierInvoiceItem, CreateInvoiceItemCommand>().ReverseMap();
            CreateMap<SupplierInvoiceItem, UpdateInvoiceItemCommand>().ReverseMap();
            CreateMap<SupplierInvoiceItem, CreateBatchCommand>().ReverseMap();
            CreateMap<SupplierInvoiceItem, CreateAtSupplierInventTransactionCommand>()
                .ForMember(x=>x.Quantity , o=> o.MapFrom(i =>(double) i.Quantity))
                
                .ReverseMap();
            CreateMap<CreateOrUpdateInventCommand, SupplierInvoiceItem>()
                .ForMember(x=>x.Quantity , o=> o.MapFrom(i => i.PhysicalQuantity))
                .ReverseMap();
            CreateMap<SupplierInvoice, SupplierInvoiceDto>().ReverseMap();
            CreateMap<SupplierInvoiceItemDto, SupplierInvoiceItem>().ReverseMap();
            CreateMap<SupplierInvoiceItem, CreateBatchCommand>()
                .ForMember(x=>x.ProductFullName, o=> o.MapFrom(i=> i.ProductName))
                .ForMember(x=>x.PurchaseDiscountRatio, o=> o.MapFrom(i=> i.Discount))
                .ForMember(x=>x.SalesUnitPrice, o=> o.MapFrom(i=> i.SalePrice))
               
                .ReverseMap();
            CreateMap<SupplierInvoiceItem, CreateOrUpdateInventCommand>()
                .ForMember(x=>x.ProductFullName, o=> o.MapFrom(i=> i.ProductName))
                .ForMember(x=>x.PurchaseDiscountRatio, o=> o.MapFrom(i=> i.Discount))
                .ForMember(x=>x.SalesUnitPrice, o=> o.MapFrom(i=> i.SalePrice))
                .ForMember(x=>x.PhysicalQuantity, o=> o.MapFrom(i=> i.Quantity))

                .ReverseMap();
            CreateMap<SupplierInvoiceItem, CreateAtSupplierInventTransactionCommand>()
                .ForMember(x=>x.ProductFullName, o=> o.MapFrom(i=> i.ProductName))
               
                .ReverseMap();
        }
        private string GetInvoiceStatus(int status)
        {
            return status switch
            {
                1 => "EN ATTENTE",
                2 => "Envoyée",
                3 => "Acceptée",
                4 => "En cours de traitement",
                5 => "En route",
             
                _ => String.Empty
            };
        }
    }
}