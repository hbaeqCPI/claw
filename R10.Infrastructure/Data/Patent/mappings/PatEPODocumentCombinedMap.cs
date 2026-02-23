using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPODocumentCombinedMap : IEntityTypeConfiguration<PatEPODocumentCombined>
    {
        public void Configure(EntityTypeBuilder<PatEPODocumentCombined> builder)
        {
            builder.ToTable("tblPatEPODocumentCombined");
            builder.HasKey(d => new { d.KeyId });
            builder.HasOne(vd => vd.DocDocument).WithMany(vd => vd.PatEPODocumentCombineds).HasForeignKey(vd => vd.CombinedDocId).HasPrincipalKey(pk => pk.DocId).IsRequired(false);
        }
    }
}
