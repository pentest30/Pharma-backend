using System.Linq;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.IS4Admin.Services
{
    public class AuthorizedIdsUser :ISAuthorizedIdsUser
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;

        public AuthorizedIdsUser(ICommandBus commandBus, ICurrentUser currentUser)
        {
            _commandBus = commandBus;
            _currentUser = currentUser;
        }
        public async Task<bool> IsAuthorized()
        {
            var identity =await _commandBus.SendAsync(new GetUserQuery() { IncludeRoles = true, Id = _currentUser.UserId });
            if (identity != null && identity.UserRoles.Any(x => x.Role.Name.Equals("SuperAdmin")))
            {
                return true;
            }

            return false;
        }
    }
}