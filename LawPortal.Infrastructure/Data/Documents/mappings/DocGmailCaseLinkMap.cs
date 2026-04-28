using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocGmailCaseLinkMap : IEntityTypeConfiguration<DocGmailCaseLink>
    {
        public void Configure(EntityTypeBuilder<DocGmailCaseLink> builder)
        {
            builder.ToTable("tblDocGmailCaseLink");
        }
    }
}
