using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Documents;


namespace R10.Infrastructure.Data.Documents.mappings
{
    public class DocVerificationSearchFieldMap : IEntityTypeConfiguration<DocVerificationSearchField>
    {
        public void Configure(EntityTypeBuilder<DocVerificationSearchField> builder)
        {
            builder.ToTable("tblDocVerificationSearchField");
            builder.HasOne(vd => vd.GSField).WithMany(vd => vd.DocVerificationSearchFields).HasForeignKey(vd => vd.FieldId).HasPrincipalKey(pk => pk.FieldId);
        }
    }
}
