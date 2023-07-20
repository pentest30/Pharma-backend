using System;
using System.Collections.Generic;
using System.Linq;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Users.DTOs
{
    public class UserDto
    {

        public UserDto(User user)
        {
            Id = user.Id;
            UserName = user.UserName;
            Email = user.Email;
            EmailConfirmedState = user.EmailConfirmed? "Email confirmé" : "Email non confirmé";
            EmailConfirmed = user.EmailConfirmed;
            PhoneNumber = user.PhoneNumber;
            Claims = user.Claims.Select(x => new UserClaimDto { Type = x.Type, Value = x.Value });
            OrganizationId = user.OrganizationId;
            OrganizationName = user.Organization?.Name;
            ManagerId = user.ManagerId;
            PhoneNumber = user.PhoneNumber;
            if (user.UserRoles==null|| !user.UserRoles.Any()) return;
            Roles = string.Join(",", user.UserRoles.Select(x => x.Role.Name));
            UserRoles = user.UserRoles.Select(x => x.RoleId).ToArray();
            

        }
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
        public string EmailConfirmedState { get; set; }
        public string PhoneNumber { get; set; }
        public string Roles { get; set; }
        public bool EmailConfirmed { get; set; }
        public Guid? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid[] UserRoles { get; set; }
        public  IEnumerable<UserClaimDto> Claims { get; set; }
        public Guid? ManagerId { get; set; }
    }
}
