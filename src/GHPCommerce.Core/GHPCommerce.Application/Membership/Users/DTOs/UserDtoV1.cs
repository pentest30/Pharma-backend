using System;

namespace GHPCommerce.Application.Membership.Users.DTOs
{
    public class UserDtoV1
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }
}
