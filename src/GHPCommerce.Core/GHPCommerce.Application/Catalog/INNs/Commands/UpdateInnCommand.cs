using FluentValidation;

namespace GHPCommerce.Application.Catalog.INNs.Commands
{
    public class UpdateInnCommand : CreateInnCommand
    {
    }

    public class UpdateInnCommandValidator : AbstractValidator<UpdateInnCommand>
    {
        public UpdateInnCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();

        }
    }
}
