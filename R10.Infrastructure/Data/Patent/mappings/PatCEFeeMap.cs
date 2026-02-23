using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCEFeeMap : IEntityTypeConfiguration<PatCEFee>
    {
        public void Configure(EntityTypeBuilder<PatCEFee> builder)
        {
            builder.ToTable("tblPatCEFee");
            builder.HasKey("FeeSetupId");
            builder.HasIndex(c => new { c.CEFeeSetupName }).IsUnique();
            builder.HasMany(f => f.Client).WithOne(c => c.PatCEFee).HasForeignKey(c => c.CEFeeSetupName).HasPrincipalKey(f => f.CEFeeSetupName);
        }
    }
}
