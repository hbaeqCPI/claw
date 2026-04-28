using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data
{
    public class NotificationConnectionMap : IEntityTypeConfiguration<NotificationConnection>
    {
        public void Configure(EntityTypeBuilder<NotificationConnection> builder)
        {
            builder.ToTable("tblCPiNotificationConnection");
        }
    }
}
