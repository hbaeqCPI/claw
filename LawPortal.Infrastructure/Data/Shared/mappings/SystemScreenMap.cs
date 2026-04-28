using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data.Shared.mappings
{
    public class SystemScreenMap : IEntityTypeConfiguration<SystemScreen>
    {
        public void Configure(EntityTypeBuilder<SystemScreen> builder)
        {
            builder.ToTable("tblSysScreen");
            builder.HasIndex(s => new {s.ScreenCode, s.ScreenName}).IsUnique();
            // builder.HasMany(s => s.QEsMain).WithOne(qe => qe.SystemScreen); // Removed: QEsMain nav property no longer exists
        }
    }
}
