using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatOwnerInvMap : IEntityTypeConfiguration<PatOwnerInv>
    {
        public void Configure(EntityTypeBuilder<PatOwnerInv> builder)
        {
            builder.ToTable("tblPatOwnerInv");
            builder.HasIndex(o => new { o.InvId, o.OwnerID }).IsUnique();
            builder.HasOne(oi => oi.Invention).WithMany(i => i.Owners).HasForeignKey(pi => pi.InvId);
            builder.HasOne(oi => oi.Owner).WithMany(o => o.OwnerInvInventions).HasForeignKey(pi => pi.OwnerID);
        }
    }
}
