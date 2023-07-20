using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class TransactionType : AggregateRoot<Guid>
    {
        public string TransactionTypeName { get; set; }
        public TransactionTypeCode CodeTransaction { get; set; }
        public bool Blocked { get; set; }
    }
}
