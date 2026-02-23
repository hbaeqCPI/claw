using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostEstimatorCountryMap : IEntityTypeConfiguration<PatCostEstimatorCountry>
    {
        public void Configure(EntityTypeBuilder<PatCostEstimatorCountry> builder)
        {
            builder.ToTable("tblPatCostEstimatorCountry");
            builder.HasKey(ga => ga.EntityId);
            builder.HasIndex(c => new { c.KeyId, c.Country }).IsUnique();
            builder.HasOne(c => c.PatCostEstimator).WithMany(c => c.PatCostEstimatorCountries).HasPrincipalKey(c => c.KeyId).HasForeignKey(d => d.KeyId);
            builder.HasOne(c => c.PatCountry).WithMany(c => c.PatCostEstimatorCountries).HasPrincipalKey(c => c.Country).HasForeignKey(d => d.Country);
        }
    }
}
