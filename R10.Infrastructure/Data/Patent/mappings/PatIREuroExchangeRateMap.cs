using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIREuroExchangeRateMap : IEntityTypeConfiguration<PatIREuroExchangeRate>
    {
        public void Configure(EntityTypeBuilder<PatIREuroExchangeRate> builder)
        {
            builder.ToTable("tblPatIREuroExchangeRate");
            builder.Property(c => c.ExchangeId).ValueGeneratedOnAdd();
            builder.Property(m => m.ExchangeId).UseIdentityColumn();
            //builder.Property(m => m.PositionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(c => c.CurrencyType).IsUnique();
            builder.HasMany(a => a.PatIREuroExchangeRateYearlys).WithOne(c => c.PatIREuroExchangeRate).HasForeignKey(t => t.ExchangeId);
        }
    }
}
