using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data
{
    public class NotificationConnectionMap : IEntityTypeConfiguration<NotificationConnection>
    {
        public void Configure(EntityTypeBuilder<NotificationConnection> builder)
        {
            builder.ToTable("tblCPiNotificationConnection");
        }
    }
}
