using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocFileSignatureRecipientMap : IEntityTypeConfiguration<DocFileSignatureRecipient>
    {
        public void Configure(EntityTypeBuilder<DocFileSignatureRecipient> builder)
        {
            builder.ToTable("tblDocFileSignatureRecipient");
            
        }
    }
}
