using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCEStageMap : IEntityTypeConfiguration<TmkCEStage>
    {
        public void Configure(EntityTypeBuilder<TmkCEStage> builder)
        {

            builder.ToTable("tblTmkCEStage");                        
            builder.HasIndex(s => s.Stage).IsUnique();
            builder.Property(s => s.StageID).ValueGeneratedOnAdd();
            builder.Property(s => s.StageID).UseIdentityColumn();
            builder.Property(s => s.StageID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
