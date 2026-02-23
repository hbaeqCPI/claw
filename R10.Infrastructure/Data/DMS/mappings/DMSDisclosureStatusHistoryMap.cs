using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSDisclosureStatusHistoryMap : IEntityTypeConfiguration<DMSDisclosureStatusHistory>
    {
        public void Configure(EntityTypeBuilder<DMSDisclosureStatusHistory> builder)
        {

            builder.ToTable("tblDMSDisclosureStatusHistory");                        
            builder.HasIndex(h => h.LogID).IsUnique();
            builder.HasOne(h => h.Disclosure).WithMany(d => d.DMSDisclosureStatusesHistory).HasForeignKey(r => r.DMSId).HasPrincipalKey(d => d.DMSId);
        }
    }
}
