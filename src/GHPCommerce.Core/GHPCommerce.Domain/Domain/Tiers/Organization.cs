using System;
using System.Collections.Generic;
using System.Linq;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public class Organization : AggregateRoot<Guid>, IOrganization
    {
        public Organization()
        { 
            Addresses = new List<Address>();
            PhoneNumbers = new List<PhoneNumber>();
            BankAccounts = new List<BankAccount>();
            UserAccounts = new List<User>();
            Emails =new List<EmailModel>();
           
        }

        // ReSharper disable once TooManyArguments
        public void UpdateOrganization(string name, string nis, string nif, string rc,string ai, string disabledReason,
            string owner, OrganizationStatus status, OrganizationActivity activity, DateTime? establishmentDate, List<Address> addresses,
            List<PhoneNumber> phoneNumbers, List<BankAccount> bankAccounts, List<User> userAccounts,
            List<EmailModel> emails)
        {
            Name = name;
            NIS = nis;
            NIF = nif;
            RC = rc;
            DisabledReason = disabledReason;
            Owner = owner;
            OrganizationStatus = status;
            Activity = activity;
            EstablishmentDate = establishmentDate;
            AI = ai;
            UpdateAddresses(addresses);
            UpdatePhoneNumbers(phoneNumbers);
            UpdateEmails(emails);
            UpdateBankAccounts(bankAccounts);
            
        }

        private void UpdateBankAccounts(List<BankAccount> bankAccounts)
        {
            if (bankAccounts.Any())
            {
                var itemsToRemove = BankAccounts
                    .Except(bankAccounts)
                    .ToList()
                    .Clone();
                foreach (var bankAccount in bankAccounts.Where(bankAccount => bankAccounts.All(x => x.Id != bankAccount.Id)))
                    BankAccounts.Add(new BankAccount(bankAccount.BankName, bankAccount.BankCode, bankAccount.Number));
                foreach (var bankAccount in itemsToRemove)
                {
                    var item = bankAccounts.FirstOrDefault(x => x.Id == bankAccount.Id);
                    if (item == null) continue;
                    var b = BankAccounts.FirstOrDefault(x => x.Id == bankAccount.Id);
                    BankAccounts.Remove(b);
                }
            }
            else BankAccounts.Clear();
        }

        private void UpdateEmails(List<EmailModel> emails)
        {
            if (emails.Any())
            {
                var itemsToRemove = Emails
                    .Except(emails)
                    .ToList()
                    .Clone();
                foreach (var email in emails.Where(email => Emails.All(x => x.Id != email.Id)))
                    Emails.Add(new EmailModel {Email = email.Email});
                foreach (var email in itemsToRemove)
                {
                    var item = emails.FirstOrDefault(x => x.Id == email.Id);
                    if (item == null) continue;
                    var e = Emails.FirstOrDefault(x => x.Id == email.Id);
                    Emails.Remove(e);
                }
            }
            else Emails.Clear();
        }

        private void UpdatePhoneNumbers(List<PhoneNumber> phoneNumbers)
        {
            if (phoneNumbers.Any())
            {
                var itemsToRemove = PhoneNumbers
                    .Except(phoneNumbers)
                    .ToList()
                    .Clone();
                foreach (var phoneNumber in phoneNumbers.Where(address => PhoneNumbers.All(x => x.Id != address.Id)))
                    PhoneNumbers.Add(new PhoneNumber(phoneNumber.CountryCode, phoneNumber.Number, phoneNumber.IsFax));
                foreach (var phoneNumber in itemsToRemove)
                {
                    var item = phoneNumbers.FirstOrDefault(x => x.Id == phoneNumber.Id);
                    if (item == null) continue;
                    var phone = PhoneNumbers.FirstOrDefault(x => x.Id == phoneNumber.Id);
                    PhoneNumbers.Remove(phone);
                }
            }
            else PhoneNumbers.Clear();
        }

        private void UpdateAddresses(List<Address> addresses)
        {
            if (addresses.Any())
            {
                var itemsToRemove = Addresses
                    .Except(addresses)
                    .ToList()
                    .Clone();
                foreach (var address in addresses.Where(address => Addresses.All(x => x.Id != address.Id)))
                    Addresses.Add(new Address(address.Street, address.City, address.ZipCode, address.Latitude,
                        address.Longitude, address.Country, address.Main, address.Billing, address.Shipping, address.State, address.Township));
             
                foreach (var address in itemsToRemove)
                {
                    var item = addresses.FirstOrDefault(x => x.Equals(address));
                    if (item != null)
                        continue;
                    var addToRemove = Addresses.FirstOrDefault(x => x.Id == address.Id);
                    Addresses.Remove(addToRemove);

                }
            }
            else Addresses.Clear();
        }

        public string Name { get; set; }
        public string NIS { get; set; }
        public string NIF { get; set; }
        public string RC { get; set; }
        public string DisabledReason { get; set; }
        public string Owner { get; set; }
        public OrganizationStatus OrganizationStatus { get; set; }
        public OrganizationActivity Activity { get; set; }
        public DateTime? EstablishmentDate { get; set; }
        public List<Address> Addresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public List<User> UserAccounts { get; set; }
        public List<EmailModel> Emails { get; set; }
        public Supplier Supplier { get; set; }
        public Customer Customer { get; set; }
        public string AI { get ; set; }
        public bool ECommerce { get; set; }
        public string OrganizationGroupCode { get; set; }
        public string OrganizationActivity { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
