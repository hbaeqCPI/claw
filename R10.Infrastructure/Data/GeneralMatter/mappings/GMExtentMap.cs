using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMExtentMap : IEntityTypeConfiguration<GMExtent>
    {
        public void Configure(EntityTypeBuilder<GMExtent> builder)
        {
            builder.ToTable("tblGMExtent");
            builder.Property(e => e.ExtentID).ValueGeneratedOnAdd();
            builder.Property(e => e.ExtentID).UseIdentityColumn();
            builder.Property(e => e.ExtentID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
