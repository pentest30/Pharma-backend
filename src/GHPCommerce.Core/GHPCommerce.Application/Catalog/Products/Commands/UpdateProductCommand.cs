using AutoMapper;
using FluentValidation;
using GHPCommerce.Domain.Domain.Catalog;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class UpdateProductCommand : CreateDraftProductCommand
    {
    }

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(v => v.FullName)
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
    public class UpdateProductCommandConfigurationMapping : Profile
    {
        public UpdateProductCommandConfigurationMapping()
        {
            CreateMap<UpdateProductCommand, Product>().ReverseMap();
        }
    }
}
