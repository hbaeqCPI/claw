using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPODocumentMergeGuideSubMap : IEntityTypeConfiguration<PatEPODocumentMergeGuideSub>
    {
        public void Configure(EntityTypeBuilder<PatEPODocumentMergeGuideSub> builder)
        {

            builder.ToTable("tblPatEPODocumentMergeGuideSub");
            builder.HasKey("SubId");
            builder.HasIndex(c => new { c.GuideId, c.SubFileName }).IsUnique();
            builder.HasOne(g => g.PatEPODocumentMergeGuide).WithMany(q => q.PatEPODocumentMergeGuideSubs).HasForeignKey(q => q.GuideId).HasPrincipalKey(g => g.GuideId);            
        }
    }
}
