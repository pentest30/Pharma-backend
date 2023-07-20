using GHPCommerce.Domain.Domain.Catalog;
using System;

namespace GHPCommerce.Application.Catalog.TransactionTypes.DTOs
{
    public class TransactionTypeDto 
    { 
        public string TransactionTypeName { get; set; }
        public Guid Id { get; set; }
        public bool Blocked { get; set; }
        public TransactionTypeCode CodeTransaction { get; set; }


    }
}
