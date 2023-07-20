using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Users.Commands
{
    public class UpdateUserCommand : ICommand<ValidationResult>
    {
        public User User { get; set; }
    }
}