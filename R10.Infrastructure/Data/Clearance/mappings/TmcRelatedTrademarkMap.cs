using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcRelatedTrademarkMap : IEntityTypeConfiguration<TmcRelatedTrademark>
    {
        public void Configure(EntityTypeBuilder<TmcRelatedTrademark> builder)
        {
            builder.ToTable("tblTmcRelatedTrademark");
            builder.HasKey("KeyId");
            builder.HasIndex(id => new { id.TmcId, id.TmkId }).IsUnique();
            builder.HasOne(rt => rt.Clearance).WithMany(t => t.RelatedTrademarks).HasForeignKey(k => k.TmcId).HasPrincipalKey(t => t.TmcId);
            builder.HasOne(rt => rt.ClearanceTrademark).WithMany(t => t.TmcRelatedTrademarks).HasForeignKey(k => k.TmkId).HasPrincipalKey(t => t.TmkId);
        }
    }
}
