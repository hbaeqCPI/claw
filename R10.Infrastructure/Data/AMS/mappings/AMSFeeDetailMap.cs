using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSFeeDetailMap : IEntityTypeConfiguration<AMSFeeDetail>
    {
        public void Configure(EntityTypeBuilder<AMSFeeDetail> builder)
        {
            builder.ToTable("tblAMSFee_Dtl");
            builder.HasOne(d => d.AMSFee).WithMany(f => f.AMSFeeDetail).HasForeignKey(d => d.FeeSetupId).HasPrincipalKey(f => f.FeeSetupId);
        }
    }
}
