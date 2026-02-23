using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSProjectionMap : IEntityTypeConfiguration<AMSProjection>
    {
        public void Configure(EntityTypeBuilder<AMSProjection> builder)
        {
            builder.ToTable("tblAMSProjectionDtl");
            builder.HasKey(d => d.DueID);
            builder.HasIndex(d => new { d.AnnID, d.PaymentType, d.AnnuityYear }).IsUnique();
            builder.HasOne(d => d.AMSDue).WithOne(d => d.AMSProjection)
                .HasPrincipalKey<AMSProjection>(d => new { d.AnnID, d.PaymentType, d.AnnuityYear })
                .HasForeignKey<AMSDue>(d => new { d.AnnID, d.PaymentType, d.AnnuityYear });
        }
    }
}
