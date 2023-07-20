using IdentityModel;
using IdentityServer4.Models;

namespace GHPCommerce.Infra.Identity
{
    public class ProfileWithRoleIdentityResource : IdentityResources.Profile
    {
        public ProfileWithRoleIdentityResource()
        {
            UserClaims.Add(JwtClaimTypes.Role);
        }
    }
}
