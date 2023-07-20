using System;

namespace GHPCommerce.Application.Catalog.Lists.DTOs
{
    public class ListDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal SHP { get; set; }
    }

    
}