using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSReminderSetupMap : IEntityTypeConfiguration<RMSReminderSetup>
    {
        public void Configure(EntityTypeBuilder<RMSReminderSetup> builder)
        {
            builder.ToTable("tblRMSReminderSetup");
            builder.HasKey(s => s.SetupId);
            builder.HasIndex(s => new { s.Country, s.CaseType, s.ActionType, s.ActionDue }).IsUnique();
            builder.HasOne(s => s.TmkCountry).WithMany(c => c.RMSReminderSetups).HasForeignKey(s => s.Country).HasPrincipalKey(c => c.Country);
            builder.HasOne(s => s.TmkCaseType).WithMany(c => c.RMSReminderSetups).HasForeignKey(s => s.CaseType).HasPrincipalKey(c => c.CaseType);
        }
    }
}
