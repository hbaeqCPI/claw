using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSQuestionMap : IEntityTypeConfiguration<DMSQuestion>
    {
        public void Configure(EntityTypeBuilder<DMSQuestion> builder)
        {
            builder.ToTable("tblDMSQuestion");
            builder.HasKey("DMSQuestionId");
            builder.HasOne(q => q.Disclosure).WithMany(d => d.DMSQuestions).HasForeignKey(q => q.DMSId).HasPrincipalKey(d => d.DMSId);
        }
    }
}
