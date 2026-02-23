using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatInventorInvMap : IEntityTypeConfiguration<PatInventorInv>
    {
        public void Configure(EntityTypeBuilder<PatInventorInv> builder)
        {
            builder.ToTable("tblPatInventorInv");
            builder.HasIndex(pi => new { pi.InventorID, pi.InvId }).IsUnique();
            builder.HasOne(pi => pi.InventorInvInventor).WithMany(i => i.InventorInventions).HasForeignKey(pi => pi.InventorID).HasPrincipalKey(i => i.InventorID);            
            builder.HasOne(pi => pi.InventorInvInvention).WithMany(i => i.Inventors).HasForeignKey(pi => pi.InvId).HasPrincipalKey(i => i.InvId);
            builder.HasOne(pi => pi.Remuneration).WithMany(i => i.Inventors).HasForeignKey(pi => pi.RemunerationId).HasPrincipalKey(i => i.RemunerationId);
            builder.HasMany(pi => pi.Distributions).WithOne(i => i.InventorInv).HasForeignKey(pi => pi.InventorInvID).HasPrincipalKey(i => i.InventorInvID);
            builder.HasOne(i => i.EmployeePosition).WithMany(a => a.InventorInvs).HasForeignKey(a => a.PositionId).HasPrincipalKey(i => i.PositionId);
            builder.HasOne(pi => pi.FRRemuneration).WithMany(i => i.Inventors).HasForeignKey(pi => pi.FRRemunerationId).HasPrincipalKey(i => i.FRRemunerationId);

        }
    }
}
