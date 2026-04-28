using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;


namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocQuickEmailLogMap : IEntityTypeConfiguration<DocQuickEmailLog>
    {
        public void Configure(EntityTypeBuilder<DocQuickEmailLog> builder)
        {
            builder.ToTable("tblDocQELog");
            builder.HasIndex(d => new { d.DocId, d.LogID }).IsUnique();
            builder.HasOne(vd => vd.DocDocument).WithMany(vd => vd.DocQuickEmailLogs).HasForeignKey(vd => vd.DocId).HasPrincipalKey(pk => pk.DocId);            
        }
    }
}
