using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCEFeeDetailMap : IEntityTypeConfiguration<TmkCEFeeDetail>
    {
        public void Configure(EntityTypeBuilder<TmkCEFeeDetail> builder)
        {
            builder.ToTable("tblTmkCEFee_Dtl");
            builder.HasKey("FeeDetailId");
            builder.HasIndex(c => new { c.Country, c.CaseType }).IsUnique();            
            builder.HasOne(c => c.TmkCEFee).WithMany(c =>c.TmkCEFeeDetail).HasPrincipalKey(c => c.FeeSetupId).HasForeignKey(d => d.FeeSetupId);
            builder.HasOne(c => c.SharedCurrencyType).WithMany(c=> c.CurrencyTmkCEFeeDetails).HasPrincipalKey(c => c.CurrencyTypeCode).HasForeignKey(d => d.CurrencyType);
        }
    }
}
