using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.DTOs.TransferLogs
{
    public class TransferLogDto
    {
        public string CreatedBy { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ZoneSourceId { get; set; }
        public string ZoneSourceName { get; set; }
        public Guid ZoneDestId { get; set; }
        public string ZoneDestName { get; set; }
        public TransferLogStatus Status { get; set; }
        public string TransferLogSequenceNumber { get; set; }
        public List<TransferLogItemDto> Items { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public string StockStateName { get; set; }
        public Guid StockStateSourceId { get; set; }
        public string StockStateSourceName { get; set; }

        public Guid TransferLogId { get; set; }
        public Guid StockStateId { get; set; }
    }

    public class TransferLogItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string InternalBatchNumber { get; set; }
        public Guid InventId { get; set; }
        public double Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Guid ZoneSourceId { get; set; }
        public string ZoneSourceName { get; set; }
        public Guid ZoneDestId { get; set; }
        public string ZoneDestName { get; set; }
        public Guid TransferLogId { get; set; }
        public string StockStateName { get; set; }
        public Guid Id { get; set; }
    }
}