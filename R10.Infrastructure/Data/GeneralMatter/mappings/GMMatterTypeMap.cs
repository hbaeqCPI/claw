using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.GeneralMatter;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterTypeMap : IEntityTypeConfiguration<GMMatterType>
    {
        public void Configure(EntityTypeBuilder<GMMatterType> builder)
        {
            builder.ToTable("tblGMMatterType");
            builder.Property(m => m.MatterTypeID).ValueGeneratedOnAdd();
            builder.Property(m => m.MatterTypeID).UseIdentityColumn();
            builder.Property(m => m.MatterTypeID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(m => m.GMMatters).WithOne(gm => gm.GMMatterType).HasForeignKey(gm => gm.MatterType).HasPrincipalKey(m => m.MatterType);
        }
    }
}
