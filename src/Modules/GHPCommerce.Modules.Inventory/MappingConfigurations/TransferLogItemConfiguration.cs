using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    public class TransferLogItemConfiguration: IEntityTypeConfiguration<TransferLogItem>
    {
        public void Configure(EntityTypeBuilder<TransferLogItem> builder)
        {
            builder.ToTable("TransferLogItem", schema: "inventory");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");

        }
    }
}