using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFReminderSetupDocMap : IEntityTypeConfiguration<FFReminderSetupDoc>
    {
        public void Configure(EntityTypeBuilder<FFReminderSetupDoc> builder)
        {
            builder.ToTable("tblFFReminderSetupDoc");
            builder.HasKey(d => d.SetUpDocId);
            builder.HasOne(d => d.FFReminderSetup).WithMany(d => d.FFReminderSetupDocs).HasPrincipalKey(d => d.SetupId).HasForeignKey(d => d.SetupId);
            builder.HasOne(d => d.FFDoc).WithMany(d => d.FFReminderSetupDocs).HasPrincipalKey(d => d.DocId).HasForeignKey(d => d.DocId);
        }
    }
}