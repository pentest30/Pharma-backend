namespace GHPCommerce.WebApi.Models.Users
{
    public class RegistrationModel
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
