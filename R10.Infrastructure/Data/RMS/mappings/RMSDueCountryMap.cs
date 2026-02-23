using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSDueCountryMap : IEntityTypeConfiguration<RMSDueCountry>
    {
        public void Configure(EntityTypeBuilder<RMSDueCountry> builder)
        {
            builder.ToTable("tblRMSDueCountry");
            builder.HasKey(d => d.DueCountryId);
            builder.HasIndex(d => new { d.DueId, d.Country }).IsUnique();
            builder.HasOne(d => d.RMSDue).WithMany(t => t.RMSDueCountries).HasForeignKey(c => c.DueId).HasPrincipalKey(d => d.DueId);
            builder.HasOne(d => d.TmkCountry).WithMany(c => c.RMSDueCountries).HasForeignKey(d => d.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
