using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSQuestionGuideChildMap : IEntityTypeConfiguration<DMSQuestionGuideChild>
    {
        public void Configure(EntityTypeBuilder<DMSQuestionGuideChild> builder)
        {

            builder.ToTable("tblDMSQuestionGuideChild");
            builder.HasKey("ChildId");
            builder.HasIndex(c => new { c.QuestionId , c.Description }).IsUnique();
            builder.HasOne(g => g.DMSQuestionGuide).WithMany(q => q.DMSQuestionGuideChildren).HasForeignKey(q => q.QuestionId).HasPrincipalKey(g => g.QuestionId);
            builder.HasMany(g => g.DMSQuestions).WithOne(q => q.DMSQuestionGuideChild).HasForeignKey(q => q.ChildId).HasPrincipalKey(g => g.ChildId);
        }
    }
}
