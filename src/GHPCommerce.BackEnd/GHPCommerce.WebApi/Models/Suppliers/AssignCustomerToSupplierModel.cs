using System;
using System.Collections.Generic;
using AutoMapper;
using FluentValidation;
using GHPCommerce.Application.Tiers.Customers.Commands;
using GHPCommerce.Application.Tiers.Suppliers.Commands;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;
using Microsoft.EntityFrameworkCore.Internal;

namespace GHPCommerce.WebApi.Models.Suppliers
{
    public class AssignCustomerToSupplierModel
    {
        public Guid OrganizationId { get; set; }
        public bool? OnlineCustomer { get; set; }
        public bool? IsPickUpLocation { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public List<Guid> AllowedProductClasses { get; set; }
        public Guid? TaxGroupId { get; set; }
        public bool? QuotaEligibility { get; set; }
        public Guid? DefaultSalesPerson { get; set; }
        public Guid? DefaultSalesGroup { get; set; }
        public Guid? DefaultDeliverySector { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        public decimal Dept { get; set; }
        public decimal LimitCredit { get; set; }
        public ConventionType ConventionType { get; set; }
        /// <summary>
        /// écheance
        /// </summary>
        public int PaymentDeadline { get; set; }

        public CustomerState CustomerState { get; set; }
        public string Code { get; set; }
        public bool IsCustomer { get; set; }
        public PaymentMode PaymentMode { get; set; }
    }
    public class SupplierConfigMapping : Profile
    {
        public SupplierConfigMapping()
        {

            CreateMap<CreateSupplierCommand, AssignCustomerToSupplierModel>()
                .ForMember(x => x.LimitCredit, o=> o.MapFrom(x => x.LimitCredit))
                .ReverseMap();
            CreateMap<UpdateSupplierCustomerCommand, AssignCustomerToSupplierModel>().ReverseMap();

        }
    }
    public class AssignCustomerToSupplierModelModelValidator : AbstractValidator<AssignCustomerToSupplierModel>
    {
        private readonly ICommandBus _commandBus;

        public AssignCustomerToSupplierModelModelValidator(ICommandBus commandBus)
        {
            _commandBus = commandBus;


            RuleFor(v => v.AllowedProductClasses)
                .Must(x=>x!=null && x.Any())
                .WithMessage("Aucune classe de produit autorisée n'a été sélectionnée");
        }
        
    }

}
