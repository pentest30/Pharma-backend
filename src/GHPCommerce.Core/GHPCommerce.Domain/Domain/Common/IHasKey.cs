namespace GHPCommerce.Domain.Domain.Common
{
    public interface IHasKey<T>
    {
        T Id { get; set; }
    }
}
