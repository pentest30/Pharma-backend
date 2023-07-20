namespace GHPCommerce.IS4Admin.Models.RoleModels
{
    using System.Collections.Generic;
    using GHPCommerce.Domain.Domain.Identity;

    public class UsersModel
    {
        public Role Role { get; set; }

        public List<User> Users { get; set; }
    }
}
