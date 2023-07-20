using System;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class SendOrderByPharmacistCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? ExpectedShippingDate { get; set; }
        public OrderType OrderType { get; set; }
        public decimal TTC { get; set; }
        public decimal THT { get; set; }
        public decimal Net { get; set; }
        public decimal OrderBenefit { get; set; }
        public decimal OrderBenefitRate { get; set; }
        public bool ToBeRespected { get; set; }
        public bool IsSpecialOrder { get; set; }

        public string RefDocument { get; set; }
        public bool OnlineOrder { get; set; }
        public Guid?  DefaultSalesPerson { get; set; }
    }
}