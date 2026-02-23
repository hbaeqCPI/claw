using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcQuestionGuideChildMap : IEntityTypeConfiguration<TmcQuestionGuideChild>
    {
        public void Configure(EntityTypeBuilder<TmcQuestionGuideChild> builder)
        {
            builder.ToTable("tblTmcQuestionGuideChild");
            builder.HasKey("ChildId");
            builder.HasIndex(c => new { c.QuestionId, c.Description }).IsUnique();
            builder.HasOne(g => g.TmcQuestionGuide).WithMany(q => q.TmcQuestionGuideChildren).HasForeignKey(q => q.QuestionId).HasPrincipalKey(g => g.QuestionId);
        }
    }
}