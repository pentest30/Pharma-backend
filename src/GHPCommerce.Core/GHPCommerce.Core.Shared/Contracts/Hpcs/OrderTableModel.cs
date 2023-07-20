using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Hpcs
{
    public class OrderTableModel
    {
        public int OrderId { get; set; }

        public DateTime? OrderDate { get; set; }

        public int? UserSubId { get; set; }

        public bool? Recieved { get; set; }
        //Sorry Received

        public DateTime? RecivedDatetime { get; set; }

        public int? SpecialUserAccountId { get; set; }

        public decimal? TotalAmount { get; set; }

        public string UserName { get; set; }
        public decimal? TotalNetAmountPNET { get; set; }
        public DateTime? ValidationTimePNET { get; set; }
        public string OrderIdPNET { get; set; }
        public string StatusPNET { get; set; }
        public decimal? DiscTotValuePNET { get; set; }
        public OrderTableModel()
        { }
        public List<OrderLineModel> Lines { get; set; }

    }
}