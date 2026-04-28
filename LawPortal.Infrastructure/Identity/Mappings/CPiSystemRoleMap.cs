using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
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
