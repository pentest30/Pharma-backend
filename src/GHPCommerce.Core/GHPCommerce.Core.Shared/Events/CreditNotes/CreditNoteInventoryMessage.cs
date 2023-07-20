using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;

namespace GHPCommerce.Core.Shared.Events.CreditNotes
{
    public class CreditNoteInventoryMessage
    {
        public CreditNoteInventoryMessage()
        {
            Items = new List<CreditNoteItemForEvent>();
        }
        public Guid CreditNoteId { get; set; }
        public Guid CorrelationId { get; set; }
        public List<CreditNoteItemForEvent> Items { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; }
        public string RefDoc { get; set; }
    }
}