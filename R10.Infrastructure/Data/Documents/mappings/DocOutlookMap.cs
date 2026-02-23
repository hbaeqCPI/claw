using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocOutlookMap : IEntityTypeConfiguration<DocOutlook>
    {
        public void Configure(EntityTypeBuilder<DocOutlook> builder)
        {
            builder.ToTable("tblDocOutlook");
        }
    }
}
