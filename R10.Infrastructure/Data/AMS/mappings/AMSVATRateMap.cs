using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSVATRateMap : IEntityTypeConfiguration<AMSVATRate>
    {
        public void Configure(EntityTypeBuilder<AMSVATRate> builder)
        {
            builder.ToTable("tblAMSVATRate");
            builder.HasKey(v => v.VATRateId);
            builder.HasIndex(v => v.Country).IsUnique();
            builder.HasOne(v => v.PatCountry).WithMany(c => c.AMSVATRate).HasForeignKey(v => v.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
