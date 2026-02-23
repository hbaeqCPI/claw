using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCECountryCostChildMap : IEntityTypeConfiguration<PatCECountryCostChild>
    {
        public void Configure(EntityTypeBuilder<PatCECountryCostChild> builder)
        {
            builder.ToTable("tblPatCECountryCostChild");
            builder.HasKey("CCId");
            builder.HasIndex(c => new { c.CDescription }).IsUnique();            
            builder.HasOne(c => c.PatCECountryCost).WithMany(c =>c.PatCECountryCostChilds).HasPrincipalKey(c => c.CostId).HasForeignKey(d => d.CostId);
            builder.HasOne(c => c.PatCurrencyType).WithMany(c => c.CurrencyPatCECountryCostChilds).HasPrincipalKey(c => c.CurrencyTypeCode).HasForeignKey(d => d.CurrencyType);
        }
    }
}
