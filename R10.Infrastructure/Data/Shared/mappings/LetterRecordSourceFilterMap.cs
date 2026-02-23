using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterRecordSourceFilterMap : IEntityTypeConfiguration<LetterRecordSourceFilter>
    {
        public void Configure(EntityTypeBuilder<LetterRecordSourceFilter> builder)
        {
            builder.ToTable("tblLetRecSourceFilter");
            builder.HasOne(f => f.LetterRecordSource).WithMany(r => r.LetterRecordSourceFilters);
        }

    }
}
