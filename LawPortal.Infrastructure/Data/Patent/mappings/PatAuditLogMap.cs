using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatAuditLogMap : IEntityTypeConfiguration<PatAuditLog>
    {
        public void Configure(EntityTypeBuilder<PatAuditLog> builder)
        {
            builder.ToTable("tblPatAuditLog");
            builder.HasKey(e => e.AuditLogId);
            builder.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
            builder.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
        }
    }
}
