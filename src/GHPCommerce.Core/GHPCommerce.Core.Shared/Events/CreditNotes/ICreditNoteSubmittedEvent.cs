using GHPCommerce.Core.Shared.Events.DeliveryReceipts;
using System;
using System.Collections.Generic;
using System.Text;

namespace GHPCommerce.Core.Shared.Events.CreditNotes
{
    public interface ICreditNoteSubmittedEvent
    {
        Guid CreditNoteId { get; }
        Guid CorrelationId { get; }
        IList<CreditNoteItemForEvent> ItemEvents { get; }
        public Guid OrganizationId { get; }
        public Guid UserId { get; set; }
        public string RefDoc { get; set; }
    }
}
