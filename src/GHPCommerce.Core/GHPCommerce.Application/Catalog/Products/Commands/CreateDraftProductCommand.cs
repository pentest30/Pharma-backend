using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.ValueObjects;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class CreateDraftProductCommand : ICommand<ValidationResult>
    {
        public string Code { get; set; }
        public string FullName { get; set; }
       
        public string Description { get; set; }
        public string RegistrationNumber { get; set; }
        public decimal PublicPrice { get; set; }
        public decimal ReferencePrice { get; set; }
        public bool Imported { get; set; }
        public bool Refundable { get; set; }
        public bool Psychotropic { get; set; }
        public bool Thermolabile { get; set; }
        public bool Removed { get; set; }
        public bool ForHospital { get; set; }
        public bool Princeps { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PackagingContent { get; set; }
        public string PackagingContentUnit { get; set; }
        public string DciConcat { get; set; }
        public string DosageConcat { get; set; }
        public int Packaging { get; set; }
        public string DefaultLocation { get; set; }
        public Guid? ProductClassId { get; set; }
        public Guid? TherapeuticClassId { get; set; }
        public Guid? PharmacologicalClassId { get; set; }
        public Guid? INNCodeId { get; set; }
        public Guid TaxGroupId { get; set; }
        public Guid? BrandId { get; set; }
        public Guid ManufacturerId { get; set; }
        public Guid? ListId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public Guid Id { get; set; }
        public ProductState ProductState { get; set; }
        public ICollection<ImageItem> Images { get; set; }
        public Guid? PackagingId { get; set; }
        public string ExternalCode { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public bool Quota  { get; set; }
    }

    public class CreateDraftProductCommandValidator : AbstractValidator<CreateDraftProductCommand>
    {
        public CreateDraftProductCommandValidator()
        {
            RuleFor(v => v.FullName)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.Code)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ManufacturerId)
                .Must(x => !x.Equals(default));
            RuleFor(v => v.ProductClassId)
                .Must(x => !x.Equals(default));
            RuleFor(v => v.TaxGroupId)
                .Must(x => !x.Equals(default));
        }
    }



}
