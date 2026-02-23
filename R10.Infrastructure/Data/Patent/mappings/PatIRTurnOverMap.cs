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
    public class PatIRTurnOverMap : IEntityTypeConfiguration<PatIRTurnOver>
    {
        public void Configure(EntityTypeBuilder<PatIRTurnOver> builder)
        {
            builder.ToTable("tblPatIRTurnOver");
            builder.Property(c => c.TurnOverId).ValueGeneratedOnAdd();
            builder.Property(m => m.TurnOverId).UseIdentityColumn();
            //builder.Property(m => m.PositionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(c => c.Year).IsUnique();
        }
    }
}
