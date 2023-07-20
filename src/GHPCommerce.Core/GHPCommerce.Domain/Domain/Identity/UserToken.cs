using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Domain.Domain.Identity
{
    public class UserToken : Entity<Guid>
    {
        public Guid UserId { get; set; }

        public string LoginProvider { get; set; }

        public string TokenName { get; set; }

        public string TokenValue { get; set; }
    }
}
