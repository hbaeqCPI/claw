using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class TimeTrackMap : IEntityTypeConfiguration<TimeTrack>
    {
        public void Configure(EntityTypeBuilder<TimeTrack> builder)
        {
            builder.ToTable("tblTimeTrack");
        }
    }
}
