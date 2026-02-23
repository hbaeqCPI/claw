using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIRFRStaggeringDetailMap : IEntityTypeConfiguration<PatIRFRStaggeringDetail>
    {
        public void Configure(EntityTypeBuilder<PatIRFRStaggeringDetail> builder)
        {
            builder.ToTable("tblPatIRFRStaggeringDetail");
            builder.Property(c => c.DetailId).ValueGeneratedOnAdd();
            builder.Property(m => m.DetailId).UseIdentityColumn();
            //builder.Property(m => m.PositionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            //builder.HasIndex(c => c.Year).IsUnique();
        }
    }
}
