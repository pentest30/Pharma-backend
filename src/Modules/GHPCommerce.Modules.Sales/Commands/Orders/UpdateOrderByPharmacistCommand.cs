using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.Entities;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class UpdateOrderByPharmacistCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public Guid? CustomerId { get; set; }

        //public short OrderStatus { get; set; }
        /*Calculated ;Sum(Discount Lines ) + Discount On Document
         public decimal OrderDiscount { get; set; }
                public decimal OrderTotal { get; set; }
                public string OrderNumber { get; set; }
                public short PaymentStatus { get; set; }*/
        public DateTime? ExpectedShippingDate { get; set; }
        public string RefDocument { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
/*
    public class UpdateOrderByPharmacistConfigMapping : Profile
    {
        public UpdateOrderByPharmacistConfigMapping()
        {
            CreateMap<Order, UpdateOrderByPharmacistCommand>()
                .ForMember(x => (OrderStatus) x.OrderStatus, o => o.MapFrom(i => i.OrderStatus));
            *//*.ForMember(x => (PaymentStatus)x.PaymentStatus, o => o.MapFrom(i => i.PaymentStatus)).ReverseMap();*//*

        }
    }*/
}