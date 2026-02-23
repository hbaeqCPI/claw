using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSActionReminderLogMap : IEntityTypeConfiguration<DMSActionReminderLog>
    {
        public void Configure(EntityTypeBuilder<DMSActionReminderLog> builder)
        {
            builder.ToTable("tblDMSActionReminderLog");
            builder.HasKey(a => a.KeyId);
            builder.HasOne(a => a.DMSActionDue).WithMany(a => a.ReminderLogs).HasForeignKey(a => a.ActId).HasPrincipalKey(a => a.ActId);            
        }
    }
}
