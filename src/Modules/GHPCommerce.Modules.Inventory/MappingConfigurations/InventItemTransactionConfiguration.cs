using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    public class InventItemTransactionConfiguration : IEntityTypeConfiguration<InventItemTransaction>
    {
        public void Configure(EntityTypeBuilder<InventItemTransaction> builder)
        {
            builder.ToTable("InventItemTransaction", schema: "inventory");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");

        }
    }
}
