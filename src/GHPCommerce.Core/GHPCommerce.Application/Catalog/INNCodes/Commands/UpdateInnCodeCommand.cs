using FluentValidation;

namespace GHPCommerce.Application.Catalog.INNCodes.Commands
{
    public class UpdateInnCodeCommand : CreateInnCodeCommand
    {
    }

    public class UpdateInnCodeCommandValidator : AbstractValidator<UpdateInnCodeCommand>
    {
        public UpdateInnCodeCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.FormId)
                .NotEmpty().Must(v => v != default);
            RuleFor(v => v.InnCodeDosages.Count)
                .GreaterThanOrEqualTo(0);
        }
    }
}
