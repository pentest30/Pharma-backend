namespace GHPCommerce.Domain.Domain.Catalog
{
    public enum  TransactionTypeCode : uint
    {
        STOCK_ENTRY = 10,
        STOCK_RELEASE = 20,
        STOCK_ADJUSTMENT = 30,
        STOCK_TRANSFER = 40,
    }
}
