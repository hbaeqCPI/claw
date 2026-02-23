using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocSystemMap : IEntityTypeConfiguration<DocSystem>
    {
        public void Configure(EntityTypeBuilder<DocSystem> builder)
        {
            builder.ToTable("tblDocControlSystem");
            builder.HasOne(ds=>ds.CPiSystem).WithMany(s=>s.DocSystems).HasForeignKey(ds=>ds.SystemType).HasPrincipalKey(s=>s.SystemType);
        }
    }
}
