using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Documents.mappings
{ 
    public class DocOutlookCaseLinkMap : IEntityTypeConfiguration<DocOutlookCaseLink>
    {
        public void Configure(EntityTypeBuilder<DocOutlookCaseLink> builder)
        {
            builder.ToTable("tblDocOutlookCaseLink");
        }
    }
}
