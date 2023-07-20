using System.ComponentModel;

namespace GHPCommerce.Modules.Sales.Entities
{
    public enum CancellationReason : uint
    {
        [Description("Delivery date exceeded")]
        ExceededDate = 1,
        [Description("I can't finish this sales this time ")]
        UnableFishingOrder = 2,
        [Description("I'm just sending a test order")]
        TestOrder = 3
    }
}
