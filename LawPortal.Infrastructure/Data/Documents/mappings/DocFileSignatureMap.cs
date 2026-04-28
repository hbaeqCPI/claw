using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;


namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocFileSignatureMap : IEntityTypeConfiguration<DocFileSignature>
    {
        public void Configure(EntityTypeBuilder<DocFileSignature> builder)
        {
            builder.ToTable("tblDocFileSignature");
            builder.HasOne(s=> s.DocFile).WithOne(d=> d.DocFileSignature).HasForeignKey<DocFileSignature>(k=> k.FileId);
        }
    }
}
