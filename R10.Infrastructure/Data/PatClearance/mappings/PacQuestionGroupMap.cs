using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacQuestionGroupMap : IEntityTypeConfiguration<PacQuestionGroup>
    {
        public void Configure(EntityTypeBuilder<PacQuestionGroup> builder)
        {
            builder.ToTable("tblPacQuestionGroup");
            builder.HasKey("GroupId");
            builder.HasMany(g => g.PacQuestionGuides).WithOne(q => q.PacQuestionGroup).HasForeignKey(q => q.GroupId).HasPrincipalKey(g => g.GroupId);
        }
    }
}