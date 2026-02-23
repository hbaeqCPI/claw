using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCEStageMap : IEntityTypeConfiguration<PatCEStage>
    {
        public void Configure(EntityTypeBuilder<PatCEStage> builder)
        {

            builder.ToTable("tblPatCEStage");                        
            builder.HasIndex(s => s.Stage).IsUnique();
            builder.Property(s => s.StageID).ValueGeneratedOnAdd();
            builder.Property(s => s.StageID).UseIdentityColumn();
            builder.Property(s => s.StageID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
