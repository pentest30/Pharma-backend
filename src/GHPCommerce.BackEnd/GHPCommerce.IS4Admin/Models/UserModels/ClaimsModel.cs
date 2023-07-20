namespace GHPCommerce.IS4Admin.Models.UserModels
{
    using System.Collections.Generic;
    using System.Linq;
    using GHPCommerce.Domain.Domain.Identity;

    public class ClaimsModel : ClaimModel
    {
        public List<ClaimModel> Claims { get; set; }

        public static ClaimsModel FromEntity(User user)
        {
            return new ClaimsModel
            {
                User = user,
                Claims = user.Claims?.Select(x => FromEntity(x))?.ToList(),
            };
        }
    }
}
