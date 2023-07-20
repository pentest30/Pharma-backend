using FluentValidation;

namespace GHPCommerce.Application.Catalog.Lists.Commands
{
    public class UpdateLisCommand : CreateListCommand
    {
    }

    public class UpdateLisCommandValidator : AbstractValidator<UpdateLisCommand>
    {
        public UpdateLisCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.Code)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.SHP)
                .GreaterThan(0);
        }
    }
}
