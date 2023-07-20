namespace GHPCommerce.Modules.Procurement.Entities
{
    public enum ProcurmentOrderStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Created = 10,
        /// <summary>
        /// Sent
        /// </summary>
        Saved = 20,
        /// <summary>
        /// Accepted
        /// </summary>
        Accepted = 30,
        /// <summary>
        /// Processing
        /// </summary>
        Processing = 40,
        /// <summary>
        /// Shipping
        /// </summary>
        Shipping = 50,
        /// <summary>
        /// Completed
        /// </summary>
        Completed = 60,

        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled = 70,
        /// <summary>
        /// Rejected
        /// </summary>
        Rejected = 80,
        /// <summary>
        /// Commande confirmée ou imprimée
        /// </summary>
        Prepared = 90,
        /// <summary>
        /// Ordres de préparation consolidés
        /// </summary>
        Consolidated = 100,
        /// <summary>
        /// Commande dans la zone d'expédition
        /// </summary>
        InShippingArea = 110,
        // commande supprimée
        Removed = 120,

    }
}