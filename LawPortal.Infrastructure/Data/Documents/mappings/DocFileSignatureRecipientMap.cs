using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocFileSignatureRecipientMap : IEntityTypeConfiguration<DocFileSignatureRecipient>
    {
        public void Configure(EntityTypeBuilder<DocFileSignatureRecipient> builder)
        {
            builder.ToTable("tblDocFileSignatureRecipient");
            
        }
    }
}
