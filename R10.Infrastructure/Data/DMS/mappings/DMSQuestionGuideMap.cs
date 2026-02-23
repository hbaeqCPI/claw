using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSQuestionGuideMap : IEntityTypeConfiguration<DMSQuestionGuide>
    {
        public void Configure(EntityTypeBuilder<DMSQuestionGuide> builder)
        {

            builder.ToTable("tblDMSQuestionGuide");
            builder.HasKey("QuestionId");
            builder.HasMany(g => g.DMSQuestions).WithOne(q => q.DMSQuestionGuide).HasForeignKey(q => q.QuestionId).HasPrincipalKey(g => g.QuestionId);
        }
    }
}
