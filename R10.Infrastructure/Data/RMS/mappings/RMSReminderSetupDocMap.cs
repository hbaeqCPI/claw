using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSReminderSetupDocMap : IEntityTypeConfiguration<RMSReminderSetupDoc>
    {
        public void Configure(EntityTypeBuilder<RMSReminderSetupDoc> builder)
        {
            builder.ToTable("tblRMSReminderSetupDoc");
            builder.HasKey(d => d.SetUpDocId);
            builder.HasOne(d => d.RMSReminderSetup).WithMany(d => d.RMSReminderSetupDocs).HasPrincipalKey(d => d.SetupId).HasForeignKey(d => d.SetupId);
            builder.HasOne(d => d.RMSDoc).WithMany(d => d.RMSReminderSetupDocs).HasPrincipalKey(d => d.DocId).HasForeignKey(d => d.DocId);
        }
    }
}
