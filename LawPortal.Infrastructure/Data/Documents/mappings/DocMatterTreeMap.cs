using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocMatterTreeMap : IEntityTypeConfiguration<DocMatterTree>
    {
        public void Configure(EntityTypeBuilder<DocMatterTree> builder)
        {
            builder.ToTable("tblDocControlMatterTree");
        }
    }
}
