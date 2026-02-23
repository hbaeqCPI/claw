using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiSystemRoleMap : IEntityTypeConfiguration<CPiSystemRole>
    {
        public void Configure(EntityTypeBuilder<CPiSystemRole> builder)
        {
            builder.ToTable("tblCPiSystemRoles");
            builder.HasKey(x => new { x.SystemId, x.RoleId });
            builder.HasIndex(x => x.SystemId).IsUnique(false);
        }
    }
}
