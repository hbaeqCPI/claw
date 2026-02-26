using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatAreaMap : IEntityTypeConfiguration<PatArea>
    {
        public void Configure(EntityTypeBuilder<PatArea> builder)
        {
            builder.ToTable("tblPatArea");
            builder.HasIndex(a => a.Area).IsUnique();
            builder.HasMany(a => a.PatAreaCountries).WithOne(ca => ca.Area).HasPrincipalKey(a=>a.AreaID).HasForeignKey(ca => ca.AreaID);
            // Removed during deep clean
            // builder.HasMany(c => c.AreaDisclosures).WithOne(d => d.Area).HasPrincipalKey(a=>a.AreaID).HasForeignKey(ca => ca.AreaID);
            // builder.HasMany(o => o.AreaDMSAgendas).WithOne(d => d.Area).HasPrincipalKey(a=>a.AreaID).HasForeignKey(d => d.AreaID);
        }
    }
}
