using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocGmailCaseLinkMap : IEntityTypeConfiguration<DocGmailCaseLink>
    {
        public void Configure(EntityTypeBuilder<DocGmailCaseLink> builder)
        {
            builder.ToTable("tblDocGmailCaseLink");
        }
    }
}
