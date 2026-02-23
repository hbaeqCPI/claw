using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class CustomReportMap : IEntityTypeConfiguration<CustomReport>
    {
        public void Configure(EntityTypeBuilder<CustomReport> builder)
        {
            builder.ToTable("tblCustomReport");

        }
    }
}
