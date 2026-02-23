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
    public class PatIRStaggeringMap : IEntityTypeConfiguration<PatIRStaggering>
    {
        public void Configure(EntityTypeBuilder<PatIRStaggering> builder)
        {
            builder.ToTable("tblPatIRStaggering");
            builder.Property(c => c.StaggeringId).ValueGeneratedOnAdd();
            builder.Property(m => m.StaggeringId).UseIdentityColumn();
            //builder.Property(m => m.PositionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(c => c.Name).IsUnique();
            builder.HasMany(a => a.PatIRStaggeringDetails).WithOne(c => c.PatIRStaggering).HasForeignKey(t => t.StaggeringId);
        }
    }
}
