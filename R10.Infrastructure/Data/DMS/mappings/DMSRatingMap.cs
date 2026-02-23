using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSRatingMap : IEntityTypeConfiguration<DMSRating>
    {
        public void Configure(EntityTypeBuilder<DMSRating> builder)
        {
            builder.ToTable("tblDMSRating");
            builder.Property(r => r.RatingId).ValueGeneratedOnAdd();
            builder.Property(r => r.RatingId).UseIdentityColumn();
            builder.HasIndex(r => r.Rating).IsUnique();
        }
    }
}
