namespace GHPCommerce.IS4Admin.Models.UserModels
{
    using System.Collections.Generic;
    using GHPCommerce.Domain.Domain.Identity;

    public class RolesModel
    {
        public User User { get; set; }

        public RoleModel Role { get; set; }

        public List<Role> Roles { get; set; }

        public List<RoleModel> UserRoles { get; set; }
    }
}
