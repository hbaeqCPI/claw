using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class TradeSecretActivityMap : IEntityTypeConfiguration<TradeSecretActivity>
    {
        public void Configure(EntityTypeBuilder<TradeSecretActivity> builder)
        {
            builder.ToTable("tblTradeSecretActivities");
            builder.HasKey(ts => ts.ActivityId);
        }
    }
}
