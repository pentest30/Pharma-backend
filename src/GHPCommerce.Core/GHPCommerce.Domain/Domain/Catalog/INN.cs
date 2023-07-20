using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class INN : AggregateRoot<Guid>
    {
        public INN()
        {
            InnCodeDosages = new List<INNCodeDosage>();
        }

        public INN(Guid id , string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

     
        public string Name { get; set; }
        public string Description { get; set; }

        public List<INNCodeDosage> InnCodeDosages { get; set; }
    }
}