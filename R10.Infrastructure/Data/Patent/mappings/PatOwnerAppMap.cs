using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatOwnerAppMap : IEntityTypeConfiguration<PatOwnerApp>
    {
        public void Configure(EntityTypeBuilder<PatOwnerApp> builder)
        {
            builder.ToTable("tblPatOwnerApp");
            builder.HasIndex(o => new { o.AppId, o.OwnerID }).IsUnique();
            builder.HasOne(oa => oa.CountryApplication).WithMany(a => a.Owners).HasForeignKey(pi => pi.AppId);
            builder.HasOne(oa => oa.Owner).WithMany(o => o.OwnerAppCountryApplications).HasForeignKey(pi => pi.OwnerID);
        }
    }
}
