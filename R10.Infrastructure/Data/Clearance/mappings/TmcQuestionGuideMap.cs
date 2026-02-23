using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcQuestionGuideMap : IEntityTypeConfiguration<TmcQuestionGuide>
    {
        public void Configure(EntityTypeBuilder<TmcQuestionGuide> builder)
        {
            builder.ToTable("tblTmcQuestionGuide");
            builder.HasKey("QuestionId");
            builder.HasMany(g => g.TmcQuestions).WithOne(q => q.TmcQuestionGuide).HasForeignKey(q => q.QuestionId).HasPrincipalKey(g => g.QuestionId);
        }
    }
}