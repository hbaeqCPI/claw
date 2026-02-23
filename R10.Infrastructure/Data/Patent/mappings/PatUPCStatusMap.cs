using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatUPCStatusMap : IEntityTypeConfiguration<PatUPCStatus>
    {
        public void Configure(EntityTypeBuilder<PatUPCStatus> builder)
        {
            builder.ToTable("tblPatUPCStatus");
            builder.Property(s => s.StatusId).ValueGeneratedOnAdd();
            builder.Property(m => m.StatusId).UseIdentityColumn();
            builder.HasIndex(s => s.UPCStatus).IsUnique();

        }
    }
}
