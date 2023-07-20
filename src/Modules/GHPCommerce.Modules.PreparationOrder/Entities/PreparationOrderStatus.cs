namespace GHPCommerce.Modules.PreparationOrder.Entities
{
    public enum  PreparationOrderStatus : uint
    {
        Prepared = 10,      
        Controlled = 20,
        Consolidated = 30,
        Valid = 40,
        ReadyToBeShipped=50,
        CancelledOrder=500

    }
}
