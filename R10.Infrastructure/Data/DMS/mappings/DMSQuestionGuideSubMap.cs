using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSQuestionGuideSubMap : IEntityTypeConfiguration<DMSQuestionGuideSub>
    {
        public void Configure(EntityTypeBuilder<DMSQuestionGuideSub> builder)
        {

            builder.ToTable("tblDMSQuestionGuideSub");
            builder.HasKey("SubId");
            builder.HasIndex(c => new { c.ChildId, c.Description }).IsUnique();
            builder.HasOne(g => g.DMSQuestionGuideChild).WithMany(q => q.DMSQuestionGuideSubs).HasForeignKey(q => q.ChildId).HasPrincipalKey(g => g.ChildId);
            builder.HasMany(g => g.DMSQuestions).WithOne(q => q.DMSQuestionGuideSub).HasForeignKey(q => q.SubId).HasPrincipalKey(g => g.SubId);
        }
    }
}
