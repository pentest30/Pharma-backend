using GHPCommerce.Domain.Domain.Commands;
using System;
using FluentValidation.Results;
using System.Collections.Generic;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class PrintBlCommand : IPDFCommande, ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public int PageCount { get; set; }
        public int FirstPageNumber { get; set; }
        public int TotalPageCount { get;  set; }
        public string ZonesStringByBL { get; set; } = "";
        public bool Bulk { get; set; } = false;
        public List<string> ZonesOnTopPage { get; set; } = new List<string>();
    }
}
