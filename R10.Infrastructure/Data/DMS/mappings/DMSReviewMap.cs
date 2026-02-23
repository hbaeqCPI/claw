using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSReviewMap : IEntityTypeConfiguration<DMSReview>
    {
        public void Configure(EntityTypeBuilder<DMSReview> builder)
        {
            builder.ToTable("tblDMSReview");
            builder.Property(r => r.DMSReviewId).ValueGeneratedOnAdd();
            builder.HasIndex(r => new { r.DMSId, r.DMSReviewId }).IsUnique();
            builder.HasOne(r => r.Disclosure).WithMany(r => r.Reviews).HasForeignKey(d => d.DMSId).HasPrincipalKey(r => r.DMSId);
            builder.HasOne(r => r.Rating).WithMany(r => r.Reviews).HasForeignKey(d => d.RatingId).HasPrincipalKey(r => r.RatingId);
            builder.HasOne(r => r.Contact).WithMany(r => r.Reviews).HasForeignKey(r => r.ReviewerId).HasPrincipalKey(c => c.ContactID);
            builder.HasOne(r => r.Inventor).WithMany(r => r.Reviews).HasForeignKey(r => r.ReviewerId).HasPrincipalKey(c => c.InventorID);
        }
    }
}
