using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.Sales.Models
{
    [Serializable]
    public class ShoppingCartModel
    {
        public ShoppingCartModel()
        {
           
            CreationDateTime = DateTime.Now;
        }
        public DateTime CreationDateTime { get; set; }
        public Guid CartId { get; set; }
        
        public List<ShoppingCartItemModel> ShoppingCartItems { get; set; }
    }
}
