using System;

namespace GHPCommerce.Application.Tiers.BankAccounts.DTOs
{
    public class BankAccountDto 
    {
        public Guid Id { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string Number { get; set; }
    }
}
