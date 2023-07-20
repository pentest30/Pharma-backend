using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public class BankAccount :Entity<Guid>, ICloneable
    {
        public BankAccount()
        {
        }

        public BankAccount(string bankName, string bankCode, string number)
        {
            BankName = bankName;
            BankCode = bankCode;
            Number = number;
        }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string Number { get; set; }
        public Organization Organization { get; set; }
        public Guid OrganizationId { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
