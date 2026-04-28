using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{ 
    public class DocOutlookCaseLinkMap : IEntityTypeConfiguration<DocOutlookCaseLink>
    {
        public void Configure(EntityTypeBuilder<DocOutlookCaseLink> builder)
        {
            builder.ToTable("tblDocOutlookCaseLink");
        }
    }
}
