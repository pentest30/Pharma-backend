using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;

namespace GHPCommerce.IS4Admin.Services
{
    public class ProfileService:IProfileService
    {
        protected UserManager<User> UserManager;
        private readonly ICommandBus _commandBus;
        public ProfileService(UserManager<User> userManager, ICommandBus commandBus)
        {
            UserManager = userManager;
            _commandBus = commandBus;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            //>Processing
            var user = await UserManager.GetUserAsync(context.Subject);
            var persistedUser = await _commandBus.SendAsync(new GetUserQuery {Id = user.Id, IncludeRoles = true, IncludeClaims = true});
            var claims = persistedUser
                .Claims
                .Select(persistedUserClaim => new Claim(persistedUserClaim.Type, persistedUserClaim.Value))
                .ToList();
            
            var roles = persistedUser
                .UserRoles
                .Select(role => new Claim("role", role.Role.Name))
                .ToList();
            context.IssuedClaims.AddRange(claims);
            if( persistedUser.OrganizationId != null) context.IssuedClaims.Add(new Claim("organizationId",persistedUser.OrganizationId.ToString()));
            if (persistedUser.UserName != null) context.IssuedClaims.Add(new Claim("userName", persistedUser.NormalizedUserName.ToString()));
         
            context.IssuedClaims.AddRange(roles);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //>Processing
            var user = await UserManager.GetUserAsync(context.Subject);

            context.IsActive = (user != null) ;
        }
    }

}
