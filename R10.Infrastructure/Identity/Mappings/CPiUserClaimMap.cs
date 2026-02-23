using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiUserClaimMap : IEntityTypeConfiguration<CPiUserClaim>
    {
        public void Configure(EntityTypeBuilder<CPiUserClaim> builder)
        {
            builder.ToTable("tblCPiUserClaims");
            builder.HasKey(x => new { x.Id });
            builder.HasOne(s => s.CPiUser).WithMany(u => u.CPiUserClaims).HasForeignKey(s => s.UserId).HasPrincipalKey(u => u.Id);
        }
    }
}
