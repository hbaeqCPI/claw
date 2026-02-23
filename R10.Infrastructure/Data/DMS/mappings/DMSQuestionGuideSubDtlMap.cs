using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSQuestionGuideSubDtlMap : IEntityTypeConfiguration<DMSQuestionGuideSubDtl>
    {
        public void Configure(EntityTypeBuilder<DMSQuestionGuideSubDtl> builder)
        {

            builder.ToTable("tblDMSQuestionGuideSubDtl");
            builder.HasKey("SubDtlId");
            builder.HasIndex(c => new { c.SubId, c.Description }).IsUnique();
            builder.HasOne(g => g.DMSQuestionGuideSub).WithMany(q => q.DMSQuestionGuideSubDtls).HasForeignKey(q => q.SubId).HasPrincipalKey(g => g.SubId);
        }
    }
}