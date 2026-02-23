using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterStatusMap : IEntityTypeConfiguration<GMMatterStatus>
    {
        public void Configure(EntityTypeBuilder<GMMatterStatus> builder)
        {
            builder.ToTable("tblGMMatterStatus");
            builder.Property(m => m.MatterStatusID).ValueGeneratedOnAdd();
            builder.Property(m => m.MatterStatusID).UseIdentityColumn();
            builder.Property(m => m.MatterStatusID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(m => m.GMMatters).WithOne(g=> g.GMMatterStatus).HasForeignKey(s => s.MatterStatus).HasPrincipalKey(s => s.MatterStatus);
        }
    }
}
