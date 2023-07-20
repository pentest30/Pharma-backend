using System;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Roles.DTOs
{
    public class RoleDto
    {
        public RoleDto(Role role)
        {
            Id = role.Id;
            Name = role.Name;
            NormalizedName = role.NormalizedName;
        }
        public Guid Id { get; set; }
        public  string Name { get; set; }
        public  string NormalizedName { get; set; }
    }
}