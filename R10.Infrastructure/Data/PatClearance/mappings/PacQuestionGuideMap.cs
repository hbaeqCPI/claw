using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacQuestionGuideMap : IEntityTypeConfiguration<PacQuestionGuide>
    {
        public void Configure(EntityTypeBuilder<PacQuestionGuide> builder)
        {
            builder.ToTable("tblPacQuestionGuide");
            builder.HasKey("QuestionId");
            builder.HasMany(g => g.PacQuestions).WithOne(q => q.PacQuestionGuide).HasForeignKey(q => q.QuestionId).HasPrincipalKey(g => g.QuestionId);
        }
    }
}