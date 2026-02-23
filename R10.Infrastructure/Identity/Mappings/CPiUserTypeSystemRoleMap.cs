using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiUserTypeSystemRoleMap : IEntityTypeConfiguration<CPiUserTypeSystemRole>
    {
        public void Configure(EntityTypeBuilder<CPiUserTypeSystemRole> builder)
        {
            builder.ToTable("tblCPiUserTypeSystemRoles");
            builder.HasKey(x => new { x.Id });
            builder.HasIndex(x => x.UserType).IsUnique(false);
        }
    }
}
