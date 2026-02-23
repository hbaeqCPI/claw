using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterRecordSourceMap : IEntityTypeConfiguration<LetterRecordSource>
    {
        public void Configure(EntityTypeBuilder<LetterRecordSource> builder)
        {
            builder.ToTable("tblLetRecSource");
            builder.HasOne(r => r.LetterMain).WithMany(l => l.LetterRecordSources);
            builder.HasOne(r => r.LetterDataSource).WithMany(s => s.LetterRecordSources).HasForeignKey(s => s.DataSourceId).HasPrincipalKey(s => s.DataSourceId);
            builder.HasMany(r => r.LetterRecordSourceFilters).WithOne(f => f.LetterRecordSource).HasForeignKey(f=>f.RecSourceId).HasPrincipalKey(s => s.RecSourceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
