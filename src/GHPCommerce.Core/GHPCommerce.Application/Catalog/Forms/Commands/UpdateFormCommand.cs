using FluentValidation;

namespace GHPCommerce.Application.Catalog.Forms.Commands
{
    public class UpdateFormCommand : CreateFormCommand
    {
    }

    public class UpdateFormCommandValidator : AbstractValidator<UpdateFormCommand>
    {
        public UpdateFormCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}
