using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatInventorAppMap : IEntityTypeConfiguration<PatInventorApp>
    {
        public void Configure(EntityTypeBuilder<PatInventorApp> builder)
        {
            builder.ToTable("tblPatInventorApp");
            builder.HasIndex(i => new { i.AppId, i.InventorID }).IsUnique();
            builder.HasOne(h => h.CountryApplication).WithMany(c=>c.Inventors).HasForeignKey(pi => pi.AppId).HasPrincipalKey(i => i.AppId);
            builder.HasOne(pi => pi.InventorAppInventor).WithMany(i => i.InventorCountryApplications).HasForeignKey(pi => pi.InventorID).HasPrincipalKey(i => i.InventorID);            
        }
    }
}
