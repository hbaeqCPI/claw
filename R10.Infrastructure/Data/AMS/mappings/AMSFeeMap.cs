using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSFeeMap : IEntityTypeConfiguration<AMSFee>
    {
        public void Configure(EntityTypeBuilder<AMSFee> builder)
        {
            builder.ToTable("tblAMSFee");
            builder.Property(f => f.FeeSetupId).ValueGeneratedOnAdd();
            builder.Property(f => f.FeeSetupId).UseIdentityColumn();
            builder.Property(f => f.FeeSetupId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(f => f.Client).WithOne(c => c.AMSFee).HasForeignKey(c => c.FeeSetupName).HasPrincipalKey(f => f.FeeSetupName);
        }
    }
}
