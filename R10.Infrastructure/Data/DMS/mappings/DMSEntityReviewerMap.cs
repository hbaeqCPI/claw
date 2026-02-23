using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSEntityReviewerMap : IEntityTypeConfiguration<DMSEntityReviewer>
    {
        public void Configure(EntityTypeBuilder<DMSEntityReviewer> builder)
        {
            builder.ToTable("tblDMSEntityReviewer");
            builder.HasIndex(r => new { r.EntityType, r.EntityId, r.ReviewerType, r.ReviewerId }).IsUnique();
            builder.HasOne(r => r.Client).WithMany(r => r.Reviewers).HasForeignKey(r => r.EntityId).HasPrincipalKey(c => c.ClientID);
            builder.HasOne(r => r.Area).WithMany(r => r.Reviewers).HasForeignKey(r => r.EntityId).HasPrincipalKey(c => c.AreaID);
            builder.HasOne(r => r.Contact).WithMany(r => r.Reviewers).HasForeignKey(r => r.ReviewerId).HasPrincipalKey(c => c.ContactID);
            builder.HasOne(r => r.Inventor).WithMany(r => r.Reviewers).HasForeignKey(r => r.ReviewerId).HasPrincipalKey(c => c.InventorID);
        }
    }
}
