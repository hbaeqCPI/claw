using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;


namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocResponsibleDocketingMap : IEntityTypeConfiguration<DocResponsibleDocketing>
    {
        public void Configure(EntityTypeBuilder<DocResponsibleDocketing> builder)
        {
            builder.ToTable("tblDocRespDocketing");
            builder.HasKey(d => d.RespId);
            builder.HasIndex(d => new { d.DocId, d.UserId, d.GroupId}).IsUnique();
            builder.HasOne(vd => vd.DocDocument).WithMany(vd => vd.DocResponsibleDocketings).HasForeignKey(vd => vd.DocId).HasPrincipalKey(pk => pk.DocId);            
        }
    }
}
