using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Identity;
using IdentityModel.Client;

namespace GHPCommerce.WebApi.Models.Users
{
    public class ProfileViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }

        public string Token { get; set; }

        public int Expiry { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? SupplierOrganizationId { get; set; }
        public Guid? SalesPersonId { get; set; }
        public Guid? CustomerId { get; set; }
        public String FullName { get; set; }
        public String Company { get; set; }

        public ProfileViewModel()
        {

        }

        public ProfileViewModel(User user, TokenResponse uToken = null)
        {
            Id = user.Id.ToString();

            Email = user.Email;
            Token = uToken?.AccessToken;
            Expiry = uToken.ExpiresIn;
            OrganizationId = user.OrganizationId;
            FullName = user.FirstName + user.LastName;
            Company = user.Company;
        }

        public static IEnumerable<ProfileViewModel> GetUserProfiles(IEnumerable<User> users)
        {
            var profiles = new List<ProfileViewModel> { };
            foreach (var user in users)
            {
                profiles.Add(new ProfileViewModel(user));
            }

            return profiles;
        }
    }
}
