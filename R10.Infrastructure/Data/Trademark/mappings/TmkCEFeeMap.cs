using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCEFeeMap : IEntityTypeConfiguration<TmkCEFee>
    {
        public void Configure(EntityTypeBuilder<TmkCEFee> builder)
        {
            builder.ToTable("tblTmkCEFee");
            builder.HasKey("FeeSetupId");
            builder.HasIndex(c => new { c.CEFeeSetupName }).IsUnique();
            builder.HasMany(f => f.Client).WithOne(c => c.TmkCEFee).HasForeignKey(c => c.TmkCEFeeSetupName).HasPrincipalKey(f => f.CEFeeSetupName);
        }
    }
}
