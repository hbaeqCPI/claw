using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacQuestionGuideChildMap : IEntityTypeConfiguration<PacQuestionGuideChild>
    {
        public void Configure(EntityTypeBuilder<PacQuestionGuideChild> builder)
        {
            builder.ToTable("tblPacQuestionGuideChild");
            builder.HasKey("ChildId");
            builder.HasIndex(c => new { c.QuestionId, c.Description }).IsUnique();
            builder.HasOne(g => g.PacQuestionGuide).WithMany(q => q.PacQuestionGuideChildren).HasForeignKey(q => q.QuestionId).HasPrincipalKey(g => g.QuestionId);
        }
    }
}