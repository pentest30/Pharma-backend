namespace GHPCommerce.IS4Admin.Models.RoleModels
{
    using System.Collections.Generic;
    using System.Linq;
    using GHPCommerce.Domain.Domain.Identity;

    public class ClaimsModel : ClaimModel
    {
        public List<ClaimModel> Claims { get; set; }

        public static ClaimsModel FromEntity(Role role)
        {
            return new ClaimsModel
            {
                Role = role,
                Claims = role.Claims?.Select(FromEntity)?.ToList(),
            };
        }
    }
}
