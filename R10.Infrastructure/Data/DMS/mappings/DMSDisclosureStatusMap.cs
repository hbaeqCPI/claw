using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSDisclosureStatusMap : IEntityTypeConfiguration<DMSDisclosureStatus>
    {
        public void Configure(EntityTypeBuilder<DMSDisclosureStatus> builder)
        {
            builder.ToTable("tblDMSDisclosureStatus");                        
            builder.Property(s => s.DisclosureStatusId).ValueGeneratedOnAdd();
            builder.Property(s => s.DisclosureStatusId).UseIdentityColumn();
            builder.Property(s => s.DisclosureStatusId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(s => s.Disclosures).WithOne(d => d.DMSDisclosureStatus).HasForeignKey(s => s.DisclosureStatus).HasPrincipalKey(d => d.DisclosureStatus);
        }
    }
}
