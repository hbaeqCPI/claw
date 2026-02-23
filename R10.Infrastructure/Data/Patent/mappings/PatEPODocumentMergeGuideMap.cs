using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPODocumentMergeGuideMap : IEntityTypeConfiguration<PatEPODocumentMergeGuide>
    {
        public void Configure(EntityTypeBuilder<PatEPODocumentMergeGuide> builder)
        {
            builder.ToTable("tblPatEPODocumentMergeGuide");
            builder.HasKey(d => new { d.GuideId });
            builder.HasIndex(d => new { d.MergeId, d.GuideFileName }).IsUnique();
            builder.HasOne(d => d.Map).WithMany(d => d.MergeGuides).HasPrincipalKey(d => d.MergeId).HasForeignKey(d => d.MergeId);
        }
    }
}
