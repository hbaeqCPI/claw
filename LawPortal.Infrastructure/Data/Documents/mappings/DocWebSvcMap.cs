using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;

namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocWebSvcMap : IEntityTypeConfiguration<DocWebSvc>
    {
        public void Configure(EntityTypeBuilder<DocWebSvc> builder)
        {
            builder.ToTable("tblDocWebSvc");
            builder.Property(i => i.EntityId).ValueGeneratedOnAdd();
            builder.Property(i => i.EntityId).UseIdentityColumn();
        }
    }
}
