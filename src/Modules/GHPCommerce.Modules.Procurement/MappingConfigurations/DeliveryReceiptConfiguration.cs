using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Procurement.MappingConfigurations
{
    public class DeliveryReceiptConfiguration: IEntityTypeConfiguration<DeliveryReceipt>
    {
        public void Configure(EntityTypeBuilder<DeliveryReceipt> builder)
        {
            builder.ToTable("DeliveryReceipt", schema: "procurement");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Items)
                .WithOne(x => x.DeliveryReceipt)
                .OnDelete(DeleteBehavior.Cascade);
        
          
        }
    }
}