using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostEstimatorCountryMap : IEntityTypeConfiguration<TmkCostEstimatorCountry>
    {
        public void Configure(EntityTypeBuilder<TmkCostEstimatorCountry> builder)
        {
            builder.ToTable("tblTmkCostEstimatorCountry");
            builder.HasKey(ga => ga.EntityId);
            builder.HasIndex(c => new { c.KeyId, c.Country }).IsUnique();
            builder.HasOne(c => c.TmkCostEstimator).WithMany(c => c.TmkCostEstimatorCountries).HasPrincipalKey(c => c.KeyId).HasForeignKey(d => d.KeyId);
            builder.HasOne(c => c.TmkCountry).WithMany(c => c.TmkCostEstimatorCountries).HasPrincipalKey(c => c.Country).HasForeignKey(d => d.Country);
        }
    }
}
