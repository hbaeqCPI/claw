using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;
using System;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacQuestionMap : IEntityTypeConfiguration<PacQuestion>
    {
        public void Configure(EntityTypeBuilder<PacQuestion> builder)
        {
            builder.ToTable("tblPacQuestion");
            builder.HasKey("PacQuestionId");
            builder.HasOne(q => q.Clearance).WithMany(c => c.PacQuestions).HasForeignKey(q => q.PacId).HasPrincipalKey(c => c.PacId);
        }
    }
}