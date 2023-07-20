using FluentValidation;

namespace GHPCommerce.Application.Catalog.Dosages.Commands
{
    public class UpdateDosageCommand : CreateDosageCommand
    {
    }

    public class UpdateDosageCommandValidator : AbstractValidator<UpdateDosageCommand>
    {
        public UpdateDosageCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
