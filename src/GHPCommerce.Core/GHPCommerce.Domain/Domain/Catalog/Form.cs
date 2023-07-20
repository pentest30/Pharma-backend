using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class Form: AggregateRoot<Guid>
    {
        
        public Form()
        {
            InnCodes = new List<INNCode>();
        }

        public Form(Guid id, string name)
        {
            Id = id;
            Name = name.ToUpper();
        }

        public Form(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public ICollection<INNCode> InnCodes { get; set; }
    }
}