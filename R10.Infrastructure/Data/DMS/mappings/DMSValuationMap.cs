using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSValuationMap : IEntityTypeConfiguration<DMSValuation>
    {
        public void Configure(EntityTypeBuilder<DMSValuation> builder)
        {
            builder.ToTable("tblDMSValuation");
            builder.Property(r => r.DMSValId).ValueGeneratedOnAdd();
            builder.HasIndex(r => new { r.DMSId, r.DMSValId }).IsUnique();
            builder.HasOne(r => r.Disclosure).WithMany(r => r.Valuations).HasForeignKey(d => d.DMSId).HasPrincipalKey(r => r.DMSId);
            builder.HasOne(r => r.ValuationMatrices).WithMany(r => r.Valuations).HasForeignKey(d => d.ValId).HasPrincipalKey(r => r.ValId);
            builder.HasOne(r => r.Rates).WithMany(r => r.Valuations).HasForeignKey(d => d.RateId).HasPrincipalKey(r => r.RateId);
            builder.HasOne(r => r.Contact).WithMany(r => r.Valuations).HasForeignKey(r => r.ReviewerId).HasPrincipalKey(c => c.ContactID);
            builder.HasOne(r => r.Inventor).WithMany(r => r.Valuations).HasForeignKey(r => r.ReviewerId).HasPrincipalKey(c => c.InventorID);
        }
    }
}
