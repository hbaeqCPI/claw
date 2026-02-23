using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocMatterTreeMap : IEntityTypeConfiguration<DocMatterTree>
    {
        public void Configure(EntityTypeBuilder<DocMatterTree> builder)
        {
            builder.ToTable("tblDocControlMatterTree");
        }
    }
}
