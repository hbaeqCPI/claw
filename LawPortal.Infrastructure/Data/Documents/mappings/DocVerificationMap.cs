using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;


namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocVerificationMap : IEntityTypeConfiguration<DocVerification>
    {
        public void Configure(EntityTypeBuilder<DocVerification> builder)
        {
            builder.ToTable("tblDocVerification");
            builder.HasIndex(d => new { d.DocId, d.ActionTypeID, d.ActId }).IsUnique();
            builder.HasOne(vd => vd.DocDocument).WithMany(vd => vd.DocVerifications).HasForeignKey(vd => vd.DocId).HasPrincipalKey(pk => pk.DocId);
        }
    }
}
