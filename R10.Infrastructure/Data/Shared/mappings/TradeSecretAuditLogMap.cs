using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class TradeSecretAuditLogMap : IEntityTypeConfiguration<TradeSecretAuditLog>
    {
        public void Configure(EntityTypeBuilder<TradeSecretAuditLog> builder)
        {
            builder.ToTable("tblTradeSecretAuditLogs");
            builder.HasKey(l => l.AuditLogId);
            builder.HasOne(ts => ts.TradeSecretActivity).WithMany(l => l.TradeSecretAuditLogs).HasPrincipalKey(ts => ts.ActivityId).HasForeignKey(l => l.ActivityId);
        }
    }
}
