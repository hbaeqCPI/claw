using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiSystemMap : IEntityTypeConfiguration<CPiSystem>
    {
        public void Configure(EntityTypeBuilder<CPiSystem> builder)
        {
            builder.ToTable("tblCPiSystems");
            builder.HasKey(x => x.Id);
            builder.HasMany(s => s.UserSystemRoles).WithOne(u => u.CPiSystem).HasForeignKey(u => u.SystemId).HasPrincipalKey(s => s.Id).IsRequired(true);
            builder.HasMany(s => s.UserTypeSystemRoles).WithOne(u => u.System).HasForeignKey(u => u.SystemId).HasPrincipalKey(s => s.Id).IsRequired(true);
        }
    }
}
