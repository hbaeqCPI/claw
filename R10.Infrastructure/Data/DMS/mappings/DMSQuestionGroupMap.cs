using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSQuestionGroupMap : IEntityTypeConfiguration<DMSQuestionGroup>
    {
        public void Configure(EntityTypeBuilder<DMSQuestionGroup> builder)
        {
            builder.ToTable("tblDMSQuestionGroup");
            builder.HasKey("GroupId");
            builder.HasMany(g => g.DMSQuestionGuides).WithOne(q => q.DMSQuestionGroup).HasForeignKey(q => q.GroupId).HasPrincipalKey(g => g.GroupId);
        }
    }
}
