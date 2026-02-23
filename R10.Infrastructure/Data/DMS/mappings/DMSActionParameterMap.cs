using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSActionParameterMap : IEntityTypeConfiguration<DMSActionParameter>
    {
        public void Configure(EntityTypeBuilder<DMSActionParameter> builder)
        {
            builder.ToTable("tblDMSActionParameter");
            builder.HasIndex(p => new { p.ActionTypeID, p.ActionDue, p.Yr, p.Mo, p.Dy }).IsUnique();
            builder.HasOne(d => d.DMSIndicator).WithMany(disc => disc.DMSActionParameters).HasForeignKey(d => d.Indicator).HasPrincipalKey(c => c.Indicator);
        }
    }
}
