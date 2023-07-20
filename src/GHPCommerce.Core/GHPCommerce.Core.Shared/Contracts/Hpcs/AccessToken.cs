using System;

namespace GHPCommerce.Core.Shared.Contracts.Hpcs
{
    public class AccessToken
    {
        public string? Token { get; set; }
        public int Expiry { get; set; }
        public DateTime Date { get; set; }
    }
}