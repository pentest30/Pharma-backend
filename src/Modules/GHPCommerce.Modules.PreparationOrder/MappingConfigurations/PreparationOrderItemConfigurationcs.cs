using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace GHPCommerce.Modules.PreparationOrder.MappingConfigurations
{
    public class PreparationOrderItemConfigurationcs : IEntityTypeConfiguration<Entities.PreparationOrderItem>
    {
        public void Configure(EntityTypeBuilder<Entities.PreparationOrderItem> builder)
        {
            builder.ToTable("PreparationOrderItem", schema: "logistics");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.Property(m => m.PpaHT).HasColumnType("decimal(18,2)");

        }
    }
}
