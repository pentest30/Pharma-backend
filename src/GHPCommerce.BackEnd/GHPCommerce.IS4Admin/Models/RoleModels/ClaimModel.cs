namespace GHPCommerce.IS4Admin.Models.RoleModels
{
    using System;
    using GHPCommerce.Domain.Domain.Identity;

    public class ClaimModel
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public Role Role { get; set; }

        public static ClaimModel FromEntity(RoleClaim claim)
        {
            return new ClaimModel
            {
                Id = claim.Id,
                Type = claim.Type,
                Value = claim.Value,
                Role = claim.Role,
            };
        }
    }
}
