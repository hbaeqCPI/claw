using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSRecommendationMap : IEntityTypeConfiguration<DMSRecommendation>
    {
        public void Configure(EntityTypeBuilder<DMSRecommendation> builder)
        {
            builder.ToTable("tblDMSRecommendation");                        
            builder.Property(r => r.RecommendationId).ValueGeneratedOnAdd();
            //builder.Property(r => r.RecommendationId).UseIdentityColumn();
            //builder.Property(r => r.RecommendationId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
