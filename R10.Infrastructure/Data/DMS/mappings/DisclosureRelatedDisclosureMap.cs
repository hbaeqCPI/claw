using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DisclosureRelatedDisclosureMap : IEntityTypeConfiguration<DisclosureRelatedDisclosure>
    {
        public void Configure(EntityTypeBuilder<DisclosureRelatedDisclosure> builder)
        {

            builder.ToTable("tblDMSDisclosureRelatedDisclosure");
            builder.HasOne(r => r.Disclosure).WithMany(dms => dms.DisclosureRelatedDisclosures).HasForeignKey(r => r.DMSId).HasPrincipalKey(d => d.DMSId);
            builder.HasOne(r => r.RelatedDisclosure).WithMany(dms => dms.DisclosureRelateds).HasForeignKey(r => r.RelatedDMSId).HasPrincipalKey(d => d.DMSId);
        }
    }
}
