using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Documents;


namespace LawPortal.Infrastructure.Data.Documents.mappings
{
    public class DocVerificationSearchFieldMap : IEntityTypeConfiguration<DocVerificationSearchField>
    {
        public void Configure(EntityTypeBuilder<DocVerificationSearchField> builder)
        {
            builder.ToTable("tblDocVerificationSearchField");
            // builder.HasOne(vd => vd.GSField).WithMany(vd => vd.DocVerificationSearchFields).HasForeignKey(vd => vd.FieldId).HasPrincipalKey(pk => pk.FieldId); // Removed: GSField nav property no longer exists
        }
    }
}
