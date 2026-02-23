using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSRecommendationHistoryMap : IEntityTypeConfiguration<DMSRecommendationHistory>
    {
        public void Configure(EntityTypeBuilder<DMSRecommendationHistory> builder)
        {

            builder.ToTable("tblDMSRecommendationHistory");                        
            builder.HasIndex(h => h.LogID).IsUnique();
            builder.HasOne(h => h.Disclosure).WithMany(d => d.DMSRecommendationsHistory).HasForeignKey(r => r.DMSId).HasPrincipalKey(d => d.DMSId);
        }
    }
}
