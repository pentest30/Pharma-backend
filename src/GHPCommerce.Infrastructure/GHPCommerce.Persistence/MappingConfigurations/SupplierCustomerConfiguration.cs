using GHPCommerce.Domain.Domain.Tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class SupplierCustomerConfiguration : IEntityTypeConfiguration<SupplierCustomer>
    {
        public void Configure(EntityTypeBuilder<SupplierCustomer> builder)
        {
            builder.ToTable("SupplierCustomers", schema: "Tiers");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.PermittedProductClasses)
                .WithOne(x => x.SupplierCustomer);
            builder.HasOne(x => x.Supplier)
                .WithMany(x => x.SupplierCustomers).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(x => x.Customer)
                .WithMany(x => x.SupplierCustomers).OnDelete(DeleteBehavior.NoAction);
            builder
                .HasOne(x => x.Sector)
                .WithMany(x => x.SupplierCustomers)
                .OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(x=>x.DefaultDeliverySector);
            builder.Property(m => m.Dept).HasColumnType("decimal(18,2)");
            builder.Property(m => m.LimitCredit).HasColumnType("decimal(18,2)");
            builder.Property(m => m.MonthlyObjective).HasColumnType("decimal(18,2)");

        }
    }
}