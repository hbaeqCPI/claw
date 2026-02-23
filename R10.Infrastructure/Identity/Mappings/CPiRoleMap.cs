using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiRoleMap : IEntityTypeConfiguration<CPiRole>
    {
        public void Configure(EntityTypeBuilder<CPiRole> builder)
        {
            builder.ToTable("tblCPiRoles");
            builder.HasKey(x => x.Id);
            builder.HasMany(r => r.UserSystemRoles).WithOne(u => u.CPiRole).HasForeignKey(x => x.RoleId).IsRequired(true);
            builder.HasMany<CPiUserTypeSystemRole>().WithOne().HasForeignKey(x => x.RoleId).IsRequired(true);
        }
    }
}
