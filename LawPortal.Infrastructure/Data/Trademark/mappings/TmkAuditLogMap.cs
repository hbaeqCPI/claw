using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkAuditLogMap : IEntityTypeConfiguration<TmkAuditLog>
    {
        public void Configure(EntityTypeBuilder<TmkAuditLog> builder)
        {
            builder.ToTable("tblTmkAuditLog");
            builder.HasKey(e => e.AuditLogId);
            builder.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
            builder.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
        }
    }
}
