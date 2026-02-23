using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiUserSystemRoleMap : IEntityTypeConfiguration<CPiUserSystemRole>
    {
        public void Configure(EntityTypeBuilder<CPiUserSystemRole> builder)
        {
            builder.ToTable("tblCPiUserSystemRoles");
            builder.HasKey(x => new { x.Id });
            builder.HasOne(s => s.CPiUser).WithMany(u => u.CPiUserSystemRoles).HasForeignKey(s => s.UserId).HasPrincipalKey(u => u.Id);
        }
    }
}
