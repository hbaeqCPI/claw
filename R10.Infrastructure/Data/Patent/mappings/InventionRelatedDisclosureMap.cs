using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class InventionRelatedDisclosureMap : IEntityTypeConfiguration<InventionRelatedDisclosure>
    {
        public void Configure(EntityTypeBuilder<InventionRelatedDisclosure> builder)
        {
            builder.ToTable("tblPatInventionRelatedDisclosure");
            builder.HasKey("KeyId");
            builder.HasIndex(id => new { id.InvId, id.DMSId }).IsUnique();
            builder.HasOne(id=> id.InventionDisclosure).WithMany(d=> d.InventionRelatedDisclosures).HasForeignKey(id=>id.DMSId).HasPrincipalKey(d=>d.DMSId);
            builder.HasOne(ir => ir.Invention).WithMany(d => d.InventionRelatedDisclosures).HasForeignKey(ir => ir.InvId).HasPrincipalKey(d => d.InvId);
        }
    }
}
