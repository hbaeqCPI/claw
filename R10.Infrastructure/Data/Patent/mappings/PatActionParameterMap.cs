using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatActionParameterMap : IEntityTypeConfiguration<PatActionParameter>
    {
        public void Configure(EntityTypeBuilder<PatActionParameter> builder)
        {
            builder.ToTable("tblPatActionParameter");
            builder.HasIndex(p => new { p.ActionTypeID, p.ActionDue, p.Yr, p.Mo, p.Dy }).IsUnique();
            builder.HasOne(p => p.PatIndicator).WithMany(i => i.ActionParameters).HasForeignKey(p => p.Indicator).HasPrincipalKey(i=>i.Indicator);
            
          
        }
    }
}
