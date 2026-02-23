using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class EPOCommunicationDocMap : IEntityTypeConfiguration<EPOCommunicationDoc>
    {
        public void Configure(EntityTypeBuilder<EPOCommunicationDoc> builder)
        {
            builder.ToTable("tblEPOCommunicationDoc");
            builder.HasIndex(a => new { a.CommunicationId, a.DocId }).IsUnique();
            builder.HasOne(h => h.Communication).WithMany(c => c.CommunicationDocs).HasForeignKey(pi => pi.CommunicationId).HasPrincipalKey(i => i.CommunicationId);
        }
    }
}
