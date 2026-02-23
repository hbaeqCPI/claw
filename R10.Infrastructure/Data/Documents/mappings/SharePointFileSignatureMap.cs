using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Documents.mappings
{
    public class SharePointFileSignatureMap : IEntityTypeConfiguration<SharePointFileSignature>
    {
        public void Configure(EntityTypeBuilder<SharePointFileSignature> builder)
        {
            builder.ToTable("tblSharePointFileSignature");
            
        }
    }
}
