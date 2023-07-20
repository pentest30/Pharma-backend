using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class Dosage : AggregateRoot<Guid>
    {
       
        public string Name { get; set; }
        public List<INNCodeDosage> InnCodeDosages { get; set; }

        public Dosage()
        {
            
        }
        public Dosage(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        public Dosage( string name)
        {
            
            Name = name;
        }
    }
}
