namespace GHPCommerce.Modules.Sales.Entities
{
    public enum OrderStatus : uint
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 10,
        /// <summary>
        /// Sent
        /// </summary>
        Sent = 20,
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
        Canceled = 70,
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
        /// <summary>
        /// Order created on AX
        /// </summary>
        CreatedOnAx = 120,
        //En cours de chargement
        Loading = 130,
        //facturée
        Invoiced = 140, 
        //En cours de prélèvement
        BeingWithdrawn = 150,
        //Prélevé
        Withdrawn = 160,
        //Accusé de réception
        AcknowledgmentOfrReceipt = 170,
        // erreur sur Ax 
        AxError = 180,
        Shipped = 190,
        CanceledAx = 200,
        // partiellement créée
        PartiallyCreatedOnAX = 210

    }
}