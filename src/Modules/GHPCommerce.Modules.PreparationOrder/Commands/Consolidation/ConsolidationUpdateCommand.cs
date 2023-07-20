using System;

namespace GHPCommerce.Modules.PreparationOrder.Commands.Consolidation
{
    public class ConsolidationUpdateCommand : ConsolidationCommand
    {
        public Guid Id { get; set; }
        public bool ReceptionExpedition { get; set; }
        public bool SentForExpedition { get; set; }
        public bool  Consolidated { get; set; }
    }
}
