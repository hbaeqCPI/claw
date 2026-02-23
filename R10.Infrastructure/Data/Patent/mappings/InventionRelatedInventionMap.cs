using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class InventionRelatedInventionMap : IEntityTypeConfiguration<InventionRelatedInvention>
    {
        public void Configure(EntityTypeBuilder<InventionRelatedInvention> builder)
        {
            builder.ToTable("tblPatInventionRelatedInvention");
            builder.HasOne(r => r.Invention).WithMany(inv => inv.InventionRelatedInventions).HasForeignKey(r => r.InvId).HasPrincipalKey(i => i.InvId);
            builder.HasOne(r => r.RelatedInvention).WithMany(inv => inv.InventionRelateds).HasForeignKey(r => r.RelatedInvId).HasPrincipalKey(i => i.InvId);
        }
    }
}
