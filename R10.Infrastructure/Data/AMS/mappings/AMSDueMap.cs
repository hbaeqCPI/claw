using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSDueMap : IEntityTypeConfiguration<AMSDue>
    {
        public void Configure(EntityTypeBuilder<AMSDue> builder)
        {
            builder.ToTable("tblAMSDue");
            builder.HasKey(d => d.DueID);
            builder.HasIndex(d => new { d.AnnID, d.PaymentType, d.AnnuityYear }).IsUnique();
        }
    }
}
