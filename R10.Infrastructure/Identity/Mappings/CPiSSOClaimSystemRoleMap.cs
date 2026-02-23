using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiSSOClaimSystemRoleMap : IEntityTypeConfiguration<CPiSSOClaimSystemRole>
    {
        public void Configure(EntityTypeBuilder<CPiSSOClaimSystemRole> builder)
        {
            builder.ToTable("tblCPiSSOClaimSystemRoles");
            builder.HasKey(x => new { x.Id });
            builder.HasIndex(x => x.Claim).IsUnique(false);
        }
    }
}
