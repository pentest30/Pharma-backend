namespace GHPCommerce.IS4Admin.Models.UserModels
{
    using System;
    using GHPCommerce.Domain.Domain.Identity;

    public class RoleModel
    {
        public Guid RoleId { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }

        public Role Role { get; set; }
    }
}
