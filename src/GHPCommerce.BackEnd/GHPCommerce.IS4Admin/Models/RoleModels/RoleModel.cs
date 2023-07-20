namespace GHPCommerce.IS4Admin.Models.RoleModels
{
    using System;
    using GHPCommerce.Domain.Domain.Identity;

    public class RoleModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public static RoleModel FromEntity(Role role)
        {
            return new RoleModel { Id = role.Id, Name = role.Name };
        }
    }
}
