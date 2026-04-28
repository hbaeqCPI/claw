using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;


namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocResponsibleReportingMap : IEntityTypeConfiguration<DocResponsibleReporting>
    {
        public void Configure(EntityTypeBuilder<DocResponsibleReporting> builder)
        {
            builder.ToTable("tblDocRespReporting");
            builder.HasKey(d => d.RRId);
            builder.HasIndex(d => new { d.DocId, d.UserId, d.GroupId}).IsUnique();
            builder.HasOne(vd => vd.DocDocument).WithMany(vd => vd.DocResponsibleReportings).HasForeignKey(vd => vd.DocId).HasPrincipalKey(pk => pk.DocId);
        }
    }
}
