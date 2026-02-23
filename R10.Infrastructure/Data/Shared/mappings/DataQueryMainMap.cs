using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DataQueryMainMap : IEntityTypeConfiguration<DataQueryMain>
    {
        public void Configure(EntityTypeBuilder<DataQueryMain> builder)
        {
            builder.ToTable("tblDQMain");
            builder.HasKey("QueryId");
            builder.HasIndex(q => q.QueryName).IsUnique();
            builder.HasOne(l => l.DataQueryCategory).WithMany(s => s.DataQueryMains).HasForeignKey(l => l.DQCatId).HasPrincipalKey(s => s.DQCatId);
        }
    }
}
