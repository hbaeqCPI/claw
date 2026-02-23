using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSCombinedMap : IEntityTypeConfiguration<DMSCombined>
    {
        public void Configure(EntityTypeBuilder<DMSCombined> builder)
        {

            builder.ToTable("tblDMSCombined");
            builder.HasIndex(r => new { r.DMSId, r.CombinedDMSId }).IsUnique();
            builder.HasOne(r => r.Disclosure).WithMany(dms => dms.DMSCombineds).HasForeignKey(r => r.DMSId).HasPrincipalKey(d => d.DMSId);
            builder.HasOne(r => r.CombinedDisclosure).WithMany(dms => dms.DisclosureCombineds).HasForeignKey(r => r.CombinedDMSId).HasPrincipalKey(d => d.DMSId);
        }
    }
}
