using FluentValidation;

namespace GHPCommerce.Application.Catalog.Brands.Commands
{
    public class UpdateBrandCommand  : CreateBrandCommand
    {
    }
    public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
    {
        public UpdateBrandCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
