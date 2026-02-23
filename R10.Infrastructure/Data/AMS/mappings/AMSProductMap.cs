using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSProductMap : IEntityTypeConfiguration<AMSProduct>
    {
        public void Configure(EntityTypeBuilder<AMSProduct> builder)
        {
            builder.ToTable("tblAMSProduct");
            builder.HasOne(ap => ap.AMSMain).WithMany(m => m.AMSProducts).HasForeignKey(ap => ap.AnnID).HasPrincipalKey(m => m.AnnID);
            builder.HasOne(ap => ap.Product).WithMany(p => p.AMSProducts).HasForeignKey(ap => ap.ProductId).HasPrincipalKey(p => p.ProductId);
        }
    }
}
