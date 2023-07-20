using System;

namespace GHPCommerce.Core.Shared.Contracts.Hpcs
{
    public class OrderLineModel
    {

        public int OrderId { get; set; }


        public string ProductId { get; set; }
        private string _productName;
        public string ProductName
        {
            get
            {
                return _productName;
                //System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_productName.ToLower());
            }
            set
            {
                _productName = value;
            }
        }
        public int? LineNum { get; set; }
        public decimal? Qty { get; set; }

        public DateTime? RequiredDeliveryDate { get; set; }

        public decimal? UnitPrice { get; set; }

        public string BatchNumber { get; set; }

        public decimal? DiscValue { get; set; }

        public DateTime ExpiryDate { get; set; }

        public decimal? DiscRatio { get; set; }

        public decimal? LineAmount { get; set; }
    }
}