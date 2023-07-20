namespace GHPCommerce.IS4Admin.Models.UserModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using GHPCommerce.Domain.Domain.Identity;

    public class ChangePasswordModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        public static ChangePasswordModel FromEntity(User user)
        {
            return new ChangePasswordModel
            {
                Id = user.Id,
                UserName = user.UserName,
            };
        }
    }
}
