using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class Manufacturer : AggregateRoot<Guid>
    {
        public Manufacturer()
        {
            Products = new List<Product>();
            Addresses = new List<Address>();
            PhoneNumbers = new List<PhoneNumber>();
        }

        public Manufacturer( string code, string name , List<Address> addresses, List<PhoneNumber> phoneNumbers, List<EmailModel> emails)
        {
           
            Code = code;
            Name = name;
            Addresses = addresses;
            PhoneNumbers = phoneNumbers;
            Emails = emails;
        }
        public Manufacturer( string code, string name, List<Address> addresses, List<PhoneNumber> phoneNumbers)
        {
           
            Code = code;
            Name = name;
            Addresses = addresses;
            PhoneNumbers = phoneNumbers;
           
        }

        public void AddAddresses(List<Address> addresses)
        {
            Addresses.AddRange(addresses);
        }
        public void AddPhoneNumbers(List<PhoneNumber> phoneNumbers)
        {
            PhoneNumbers.AddRange(phoneNumbers);
        }

        public void UpdateManufacturer(string code, string name, List<Address> addresses, List<PhoneNumber> phoneNumbers , List<EmailModel> emails)
        {
            Code = code;
            Name = name;
            Addresses = addresses;
            PhoneNumbers = phoneNumbers;
            Emails = emails;
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public List<Address> Addresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<EmailModel> Emails { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
