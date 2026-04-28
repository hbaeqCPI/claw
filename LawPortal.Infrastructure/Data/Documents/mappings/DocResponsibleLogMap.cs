using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;


namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocResponsibleLogMap : IEntityTypeConfiguration<DocResponsibleLog>
    {
        public void Configure(EntityTypeBuilder<DocResponsibleLog> builder)
        {
            builder.ToTable("tblDocResponsibleLog");
            builder.HasKey(d => d.LogId);
            builder.HasOne(vd => vd.DocDocument).WithMany(vd => vd.DocResponsibleLogs).HasForeignKey(vd => vd.DocId).HasPrincipalKey(pk => pk.DocId);
        }
    }
}
