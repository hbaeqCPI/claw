using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiSSOClaimUserMap : IEntityTypeConfiguration<CPiSSOClaimUser>
    {
        public void Configure(EntityTypeBuilder<CPiSSOClaimUser> builder)
        {
            builder.ToTable("tblCPiSSOClaimUsers");
            builder.HasKey(x => new { x.Id });
            builder.HasIndex(x => x.Claim).IsUnique(true);
        }
    }
}
