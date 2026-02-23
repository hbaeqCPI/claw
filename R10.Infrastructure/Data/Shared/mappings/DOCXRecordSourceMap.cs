using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXRecordSourceMap : IEntityTypeConfiguration<DOCXRecordSource>
    {
        public void Configure(EntityTypeBuilder<DOCXRecordSource> builder)
        {
            builder.ToTable("tblDOCXRecSource");
            builder.HasOne(r => r.DOCXMain).WithMany(l => l.DOCXRecordSources);
            builder.HasOne(r => r.DOCXDataSource).WithMany(s => s.DOCXRecordSources).HasForeignKey(s => s.DataSourceId).HasPrincipalKey(s => s.DataSourceId);
            builder.HasMany(r => r.DOCXRecordSourceFilters).WithOne(f => f.DOCXRecordSource).HasForeignKey(f=>f.RecSourceId).HasPrincipalKey(s => s.RecSourceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
