using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCECountryCostSubMap : IEntityTypeConfiguration<PatCECountryCostSub>
    {
        public void Configure(EntityTypeBuilder<PatCECountryCostSub> builder)
        {
            builder.ToTable("tblPatCECountryCostSub");
            builder.HasKey("SubId");
            builder.HasIndex(c => new { c.SDescription }).IsUnique();            
            builder.HasOne(c => c.PatCECountryCostChild).WithMany(c =>c.PatCECountryCostSubs).HasPrincipalKey(c => c.CCId).HasForeignKey(d => d.CCId);
        }
    }
}
