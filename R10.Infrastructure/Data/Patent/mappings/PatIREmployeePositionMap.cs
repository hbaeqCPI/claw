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
    public class PatIREmployeePositionMap : IEntityTypeConfiguration<PatIREmployeePosition>
    {
        public void Configure(EntityTypeBuilder<PatIREmployeePosition> builder)
        {
            builder.ToTable("tblPatIREmployeePosition");
            builder.Property(c => c.PositionId).ValueGeneratedOnAdd();
            builder.Property(m => m.PositionId).UseIdentityColumn();
            //builder.Property(m => m.PositionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(c => c.Position).IsUnique();
        }
    }
}
