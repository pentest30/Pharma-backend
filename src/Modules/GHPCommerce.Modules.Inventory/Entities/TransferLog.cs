using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Inventory.Entities
{
    public class TransferLog : AggregateRoot<Guid>, IEntitySequenceNumber
    {
        public TransferLog()
        {
            Items = new List<TransferLogItem>();
        }

        public int SequenceNumber { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ZoneSourceId { get; set; }
        public string ZoneSourceName { get; set; }
        public Guid ZoneDestId { get; set; }
        public string ZoneDestName { get; set; }
        public Guid StockStateId { get; set; }
        public string StockStateName { get; set; }
        public Guid StockStateSourceId { get; set; }
        public string StockStateSourceName { get; set; }

        public TransferLogStatus Status { get; set; }
        public List<TransferLogItem> Items { get; set; }
        public string DocumentRef => "FL-"+ CreatedDateTime.ToString("yy-MM-dd").Substring(0,2)
                                          +"/" +"0000000000".Substring(0,10-SequenceNumber.ToString().Length)+ SequenceNumber;


    }
}