using System;
using System.Collections.Generic;
using GHPCommerce.Ecommerce.Helpers.UI;

namespace GHPCommerce.Ecommerce.Models
{
    public class PagerViewModel<T> 
    {
        public Guid Id { get; set; }
        public IEnumerable<T> Items { get; set; }
        public Pager Pager { get; set; }
    }
}
