using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles", schema: "ids");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasData(new List<UserRole>
            {
                new UserRole
                {
                    Id = Guid.Parse("12837D3D-793F-EB11-BECC-5CEA1D05F660"),
                    UserId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F660"),
                    RoleId = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0209b")
                   
                },
            });
        }
    }
}
