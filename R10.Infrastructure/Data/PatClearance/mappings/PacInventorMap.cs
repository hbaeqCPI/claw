using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacInventorMap : IEntityTypeConfiguration<PacInventor>
    {
        public void Configure(EntityTypeBuilder<PacInventor> builder)
        {

            builder.ToTable("tblPacInventor");
            builder.HasIndex(d => new { d.PacId, d.InventorID }).IsUnique();
            builder.HasOne(di => di.InventorPacClearance).WithMany(d=>d.Inventors).HasForeignKey(a => a.PacId).HasPrincipalKey(i => i.PacId);

        }
    }
}
