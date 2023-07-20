using GHPCommerce.Modules.HumanResource.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.HumanResource.MappingConfigurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employee", schema: "hr");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");

        }
    }
}