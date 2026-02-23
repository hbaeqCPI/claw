using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatLicenseeMap : IEntityTypeConfiguration<PatLicensee>
    {
        public void Configure(EntityTypeBuilder<PatLicensee> builder)
        {
            builder.ToTable("tblPatLicensee");
            builder.HasIndex(l => new { l.AppId, l.Licensee }).IsUnique();
            builder.HasIndex(l => l.Licensee);
            builder.HasIndex(l => l.Licensor);
            builder.HasOne(h => h.CountryApplication).WithMany(c=>c.Licensees).HasForeignKey(l => l.AppId).HasPrincipalKey(c => c.AppId);

            
            
        }
    }
}
