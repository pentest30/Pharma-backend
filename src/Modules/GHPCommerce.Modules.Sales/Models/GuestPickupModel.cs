using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.Sales.Models
{
    public class GuestPickupModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid VendorId { get; set; }
        public string CustomerName { get; set; }
        public List<ShoppingCartItemModel> ShoppingCartItems { get; set; }
    }
}
