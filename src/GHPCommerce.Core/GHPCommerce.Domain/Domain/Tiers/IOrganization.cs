using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public interface  IOrganization 
    {

        /// <summary>
        /// gets or  sets name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// gets or  sets NIS
        /// </summary>
        string NIS { get; set; }
        /// <summary>
        /// gets or  sets NIF
        /// </summary>
        string NIF { get; set; }
        /// <summary>
        /// gets or  sets RC
        /// </summary>
        string RC { get; set; }
        /// <summary>
        /// gets or  sets AI
        /// </summary>
        string AI { get; set; }
        /// <summary>
        /// gets or  sets disabled reason
        /// </summary>
        string DisabledReason { get; set; }
        /// <summary>
        /// gets or  sets disabled Owner
        /// </summary>
        string Owner { get; set; }
        /// <summary>
        /// gets or  sets status
        /// </summary>
        OrganizationStatus OrganizationStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        OrganizationActivity Activity { get; set; }
        /// <summary>
        /// gets or sets date of Establishment 
        /// </summary>
        DateTime? EstablishmentDate { get; set; }
        /// <summary>
        /// gets or  sets ShippingAddresses
        /// </summary>
        List<Address> Addresses { get; set; }
        /// <summary>
        /// gets or  sets phone numbers
        /// </summary>
        List<PhoneNumber> PhoneNumbers { get; set; }
        /// <summary>
        /// gets or  sets bank accounts
        /// </summary>
        List<BankAccount> BankAccounts { get; set; }
        /// <summary>
        /// gets or  sets user accounts
        /// </summary>
        List<User> UserAccounts { get; set; }

         List<EmailModel> Emails { get;  set; }
    }
}