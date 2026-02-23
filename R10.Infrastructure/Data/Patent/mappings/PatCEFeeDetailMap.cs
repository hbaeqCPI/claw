using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCEFeeDetailMap : IEntityTypeConfiguration<PatCEFeeDetail>
    {
        public void Configure(EntityTypeBuilder<PatCEFeeDetail> builder)
        {
            builder.ToTable("tblPatCEFee_Dtl");
            builder.HasKey("FeeDetailId");
            builder.HasIndex(c => new { c.EntityStatus, c.Country, c.CaseType }).IsUnique();            
            builder.HasOne(c => c.PatCEFee).WithMany(c =>c.PatCEFeeDetail).HasPrincipalKey(c => c.FeeSetupId).HasForeignKey(d => d.FeeSetupId);
        }
    }
}
