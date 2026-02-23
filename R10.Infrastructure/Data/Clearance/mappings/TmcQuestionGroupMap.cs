using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcQuestionGroupMap : IEntityTypeConfiguration<TmcQuestionGroup>
    {
        public void Configure(EntityTypeBuilder<TmcQuestionGroup> builder)
        {
            builder.ToTable("tblTmcQuestionGroup");
            builder.HasKey("GroupId");
            builder.HasMany(g => g.TmcQuestionGuides).WithOne(q => q.TmcQuestionGroup).HasForeignKey(q => q.GroupId).HasPrincipalKey(g => g.GroupId);
        }
    }
}