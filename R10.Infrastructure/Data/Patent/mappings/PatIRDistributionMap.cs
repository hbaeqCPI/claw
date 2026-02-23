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
    public class PatIRDistributionMap : IEntityTypeConfiguration<PatIRDistribution>
    {
        public void Configure(EntityTypeBuilder<PatIRDistribution> builder)
        {
            builder.ToTable("tblPatIRDistribution");
            builder.Property(c => c.DistributionId).ValueGeneratedOnAdd();
            builder.Property(m => m.DistributionId).UseIdentityColumn();
        }
    }
}
