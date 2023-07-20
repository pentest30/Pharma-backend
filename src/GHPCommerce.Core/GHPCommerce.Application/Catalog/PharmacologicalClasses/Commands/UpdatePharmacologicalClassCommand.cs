using FluentValidation;

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Commands
{
    public class UpdatePharmacologicalClassCommand : CreatePharmacologicalClassCommand
    {
    }

    public class UpdatePharmacologicalClassCommandValidator : AbstractValidator<UpdatePharmacologicalClassCommand>
    {
        public UpdatePharmacologicalClassCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
