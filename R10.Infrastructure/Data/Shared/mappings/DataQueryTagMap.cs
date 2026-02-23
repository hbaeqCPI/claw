using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DataQueryTagMap : IEntityTypeConfiguration<DataQueryTag>
    {
        public void Configure(EntityTypeBuilder<DataQueryTag> builder)
        {
            builder.ToTable("tblDQTag");
            builder.Property(d => d.DQTagId).ValueGeneratedOnAdd();
            builder.HasOne(d => d.DataQuery).WithMany(f => f.DataQueryTags).HasForeignKey(d => d.QueryId).HasPrincipalKey(d => d.QueryId);
        }
    }
}
