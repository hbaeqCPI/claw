using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class SharePointFileSignatureMap : IEntityTypeConfiguration<SharePointFileSignature>
    {
        public void Configure(EntityTypeBuilder<SharePointFileSignature> builder)
        {
            builder.ToTable("tblSharePointFileSignature");
            
        }
    }
}
