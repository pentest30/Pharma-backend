using FluentValidation;

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Commands
{
    public class UpdateTherapeuticClassCommand : CreateTherapeuticClassCommand
    {
    }
    public class UpdateTherapeuticClassCommandValidator : AbstractValidator<CreateTherapeuticClassCommand>
    {
        public UpdateTherapeuticClassCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
