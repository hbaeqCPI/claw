using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFDueCountryMap : IEntityTypeConfiguration<FFDueCountry>
    {
        public void Configure(EntityTypeBuilder<FFDueCountry> builder)
        {
            builder.ToTable("tblFFDueCountry");
            builder.HasKey(d => d.DueCountryId);
            builder.HasIndex(d => new { d.DueId, d.Source, d.Country }).IsUnique();
            builder.HasOne(d => d.FFDue).WithMany(t => t.FFDueCountries).HasForeignKey(c => c.DueId).HasPrincipalKey(d => d.DueId);
            builder.HasOne(d => d.DesCountry).WithMany(c => c.FFDueDesCountry).HasForeignKey(d => d.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
