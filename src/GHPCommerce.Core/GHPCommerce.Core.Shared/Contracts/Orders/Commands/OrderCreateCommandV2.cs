using System;
using System.Collections.Generic;
using FluentValidation;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Commands
{
    public class OrderCreateCommandV2 : ICommand<CachedOrder>
    {
        public Guid OrderId { get; set; }
        public string RefDocumentHpcs { get; set; }
        public DateTime? DateDocumentHpcs { get; set; }
        public Guid SupplierOrganizationId { get; set; }

        public Guid CustomerId { get; set; }
        public Guid? SalesPersonId { get; set; }

        public OrderType OrderType { get; set; }
        
        public string DocumentRef { get; set; }
        
        
        public bool ToBeRespected { get; set; }
        public List<OrderItemCreateCommand> orderItems { get; set; } = new List<OrderItemCreateCommand>();
    }
    public class OrderCreateCommandV2Validator : AbstractValidator<OrderCreateCommandV2>
    {
        public OrderCreateCommandV2Validator()
        {
           
            RuleFor(v => v.CustomerId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.OrderId)
                .Must(x => x != Guid.Empty);
           
            RuleFor(v => v.DocumentRef)
                .NotEmpty().When(x=>x.OrderType == OrderType.Psychotrope);
        }
    }
}