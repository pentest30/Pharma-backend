using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace GHPCommerce.Modules.PreparationOrder.MappingConfigurations
{
    public class PreparationOrderConfiguration : IEntityTypeConfiguration<Entities.PreparationOrder>
    {
        public void Configure(EntityTypeBuilder<Entities.PreparationOrder> builder)
        {
            builder.ToTable("PreparationOrder", schema: "logistics");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.PreparationOrderItems)
              .WithOne(x => x.PreparationOrder)
              .OnDelete(DeleteBehavior.Cascade)
              ;
          
          
        }
    }
}
