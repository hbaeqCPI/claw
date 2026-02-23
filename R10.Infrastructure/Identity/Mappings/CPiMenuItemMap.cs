using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiMenuItemMap : IEntityTypeConfiguration<CPiMenuItem>
    {
        public void Configure(EntityTypeBuilder<CPiMenuItem> builder)
        {
            builder.ToTable("tblCPiMenuItems");
            builder.HasKey(x => new { x.Id });
            builder.Property(prop => prop.Id).HasDefaultValueSql("newid()");
            builder.Property(prop => prop.DateCreated).HasDefaultValueSql("getdate()");
            builder.Property(prop => prop.LastUpdate).HasDefaultValueSql("getdate()");
        }
    }
}
