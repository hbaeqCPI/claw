using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;


namespace R10.Infrastructure.Data.Documents.mappings
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
