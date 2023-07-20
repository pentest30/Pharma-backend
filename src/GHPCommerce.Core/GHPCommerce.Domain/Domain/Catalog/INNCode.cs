using System;
using System.Collections.Generic;
using System.Linq;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class INNCode: AggregateRoot<Guid>
    {
        public INNCode()
        {
            InnCodeDosages = new List<INNCodeDosage>();
        }

        public string Name { get; set; }
        public Guid FormId { get; set; }
        public Form Form { get; set; }

        public INNCode(string name,  Guid formId)
        {
            Name = name;
            FormId = formId;
            InnCodeDosages = new List<INNCodeDosage>();
        }

        public void AddInnCodeLine(Guid innCodeId, Guid innId, Guid dosageId)
        {
            var line = new INNCodeDosage(innCodeId, innId, dosageId);
            if (InnCodeDosages.All(x => x != line))
                InnCodeDosages.Add(new INNCodeDosage(innCodeId, innId, dosageId));
        }

        public void UpdateInnCode(string name, Guid formId, List<INNCodeDosage> innCodeDosages)
        {
            Name = name;
            FormId = formId;
            if (innCodeDosages.Any())
                InnCodeDosages = innCodeDosages;
            else InnCodeDosages.Clear();
        }

        public List<INNCodeDosage> InnCodeDosages { get; set; }
    }
}
