using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSAgendaReviewerMap : IEntityTypeConfiguration<DMSAgendaReviewer>
    {
        public void Configure(EntityTypeBuilder<DMSAgendaReviewer> builder)
        {
            builder.ToTable("tblDMSAgendaReviewer");
            builder.HasIndex(r => new { r.AgendaId, r.ReviewerType, r.ReviewerId }).IsUnique();            
            builder.HasOne(r => r.Contact).WithMany(r => r.DMSAgendaReviewers).HasForeignKey(r => r.ReviewerId).HasPrincipalKey(c => c.ContactID);
            builder.HasOne(r => r.Inventor).WithMany(r => r.DMSAgendaReviewers).HasForeignKey(r => r.ReviewerId).HasPrincipalKey(c => c.InventorID);
        }
    }
}
