using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatTaxBaseMap : IEntityTypeConfiguration<PatTaxBase>
    {
        public void Configure(EntityTypeBuilder<PatTaxBase> builder)
        {
            builder.ToTable("tblPatTaxBase");
            builder.HasIndex(t => new { t.Country, t.TaxSchedule, t.AgentID }).IsUnique();
            builder.HasOne(t => t.Agent).WithMany(a => a.AgentPatTaxBases);
            builder.HasOne(t => t.PatCurrencyType).WithMany(c => c.CurrencyPatTaxBases).HasForeignKey(c => c.CurrencyType).HasPrincipalKey(c => c.CurrencyTypeCode);
        }
    }
}
