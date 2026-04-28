using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;


namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocDocumentMap : IEntityTypeConfiguration<DocDocument>
    {
        public void Configure(EntityTypeBuilder<DocDocument> builder)
        {
            builder.ToTable("tblDocDocument");
            builder.Property(d => d.DocId).ValueGeneratedOnAdd();
            builder.HasOne(d => d.DocFile).WithOne(f => f.DocDocument).HasForeignKey<DocDocument>(d => d.FileId);
        }
    }

    public class DocDocumentTagMap : IEntityTypeConfiguration<DocDocumentTag>
    {
        public void Configure(EntityTypeBuilder<DocDocumentTag> builder)
        {
            builder.ToTable("tblDocDocumentTag");
            builder.Property(d => d.DocTagId).ValueGeneratedOnAdd();
            builder.HasOne(d => d.DocDocument).WithMany(f => f.DocDocumentTags).HasForeignKey(d => d.DocId).HasPrincipalKey(d => d.DocId);
        }
    }
}
