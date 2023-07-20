using GHPCommerce.Domain.Domain.Tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers", schema: "Tiers");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.SupplierCustomers)
                .WithOne(x => x.Customer)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
