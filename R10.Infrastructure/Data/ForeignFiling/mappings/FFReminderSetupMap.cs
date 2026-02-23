using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFReminderSetupMap : IEntityTypeConfiguration<FFReminderSetup>
    {
        public void Configure(EntityTypeBuilder<FFReminderSetup> builder)
        {
            builder.ToTable("tblFFReminderSetup");
            builder.HasKey(s => s.SetupId);
            builder.HasIndex(s => new { s.Country, s.CaseType, s.ActionType, s.ActionDue }).IsUnique();
            builder.HasOne(s => s.PatCountry).WithMany(c => c.FFReminderSetups).HasForeignKey(s => s.Country).HasPrincipalKey(c => c.Country);
            builder.HasOne(s => s.PatCaseType).WithMany(c => c.FFReminderSetups).HasForeignKey(s => s.CaseType).HasPrincipalKey(c => c.CaseType);
        }
    }
}