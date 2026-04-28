using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocOutlookMap : IEntityTypeConfiguration<DocOutlook>
    {
        public void Configure(EntityTypeBuilder<DocOutlook> builder)
        {
            builder.ToTable("tblDocOutlook");
        }
    }
}
