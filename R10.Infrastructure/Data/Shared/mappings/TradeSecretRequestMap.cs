using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class TradeSecretRequestMap : IEntityTypeConfiguration<TradeSecretRequest>
    {
        public void Configure(EntityTypeBuilder<TradeSecretRequest> builder)
        {
            builder.ToTable("tblTradeSecretRequests");
            builder.HasKey(ts => ts.RequestId);
            builder.HasDiscriminator(ts => ts.ScreenId).HasValue<InventionTradeSecretRequest>(TradeSecretScreen.Invention);
            builder.HasDiscriminator(ts => ts.ScreenId).HasValue<DisclosureTradeSecretRequest>(TradeSecretScreen.DMSDisclosure);
            builder.HasOne(ts => ts.CPiUser).WithMany(u => u.TradeSecretRequests).HasPrincipalKey(u => u.Id).HasForeignKey(ts => ts.UserId);
        }
    }
}
