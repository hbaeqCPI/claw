using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSFaqDocMap : IEntityTypeConfiguration<DMSFaqDoc>
    {
        public void Configure(EntityTypeBuilder<DMSFaqDoc> builder)
        {

            builder.ToTable("tblDMSFAQDoc");
            builder.Property(d => d.FaqId).ValueGeneratedOnAdd();
            builder.HasIndex(r => new { r.DocName }).IsUnique();
            builder.HasOne(d => d.DocFile).WithOne(f => f.DMSFaqDoc).HasForeignKey<DMSFaqDoc>(d => d.FileId).IsRequired(false);
            builder.HasOne(i => i.DocType).WithMany(d => d.DMSFaqDocs).HasForeignKey(ti => ti.DocTypeId).HasPrincipalKey(t => t.DocTypeId).IsRequired(false);
        }
    }
}
