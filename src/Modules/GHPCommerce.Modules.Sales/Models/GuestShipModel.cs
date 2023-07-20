using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.Sales.Models
{
    public class GuestShipModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Township { get; set; }
        public string ZipCode { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Country { get; set; }
        public bool Main { get; set; }
        public bool Shipping { get; set; }
        public bool Billing { get; set; }
        public List<ShoppingCartItemModel> ShoppingCartItems { get; set; }
    }
}
