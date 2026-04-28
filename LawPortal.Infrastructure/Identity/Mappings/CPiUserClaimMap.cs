using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
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
