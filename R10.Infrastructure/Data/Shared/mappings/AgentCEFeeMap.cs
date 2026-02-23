using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class AgentCEFeeMap : IEntityTypeConfiguration<AgentCEFee>
    {
        public void Configure(EntityTypeBuilder<AgentCEFee> builder)
        {
            builder.ToTable("tblAgentCEFee");
            builder.HasIndex(ac => new { ac.AgentID, ac.Country, ac.SystemType, ac.CostType, ac.CurrencyType, ac.OriginatingLanguage, ac.TranslationType }).IsUnique();
            builder.HasOne(a => a.Agent).WithMany(f => f.AgentCEFees).HasForeignKey(t => t.AgentID).HasPrincipalKey(t => t.AgentID);
            builder.HasOne(c => c.SharedCurrencyType).WithMany(c=> c.CurrencyAgentCEFees).HasPrincipalKey(c => c.CurrencyTypeCode).HasForeignKey(d => d.CurrencyType);
        }
    }
}
