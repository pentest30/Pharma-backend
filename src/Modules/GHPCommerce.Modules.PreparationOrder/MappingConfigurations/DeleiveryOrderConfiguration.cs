using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.PreparationOrder.MappingConfigurations
{
    public class DeleiveryOrderConfiguration: IEntityTypeConfiguration<Entities.DeleiveryOrder>
    {
        public void Configure(EntityTypeBuilder<Entities.DeleiveryOrder> builder)
        {
            builder.ToTable("DeleiveryOrder", schema: "logistics");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.DeleiveryOrderItems)
                .WithOne(x => x.DeleiveryOrder)
                .OnDelete(DeleteBehavior.Cascade)
                ;

           
        }
    }
}