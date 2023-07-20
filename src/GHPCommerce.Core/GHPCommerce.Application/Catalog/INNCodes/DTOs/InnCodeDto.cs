using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Catalog;

namespace GHPCommerce.Application.Catalog.INNCodes.DTOs
{
    public class InnCodeDto
    {
        public InnCodeDto()
        {
            
        }
        public Guid Id { get; set; }
        public InnCodeDto(string name, string formName)
        {
            Name = name;
            FormName = formName;
        }
        public InnCodeDto(Guid id ,string name, string formName, Guid fromId)
        {
            Id = id;
            FormId = fromId;
            Name = name;
            FormName = formName;

        }
        public string Name { get; set; }
        public Guid FormId { get; set; }
        public string FormName { get; set; }
        public ICollection<INNCodeDosage> InnCodeDosages { get; set; }
    }
}
