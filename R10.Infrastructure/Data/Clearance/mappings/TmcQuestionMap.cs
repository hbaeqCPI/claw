using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;
using System;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcQuestionMap : IEntityTypeConfiguration<TmcQuestion>
    {
        public void Configure(EntityTypeBuilder<TmcQuestion> builder)
        {
            builder.ToTable("tblTmcQuestion");
            builder.HasKey("TmcQuestionId");
            builder.HasOne(q => q.Clearance).WithMany(c => c.TmcQuestions).HasForeignKey(q => q.TmcId).HasPrincipalKey(c => c.TmcId);
        }
    }
}