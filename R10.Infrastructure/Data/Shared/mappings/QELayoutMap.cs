using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class QELayoutMap : IEntityTypeConfiguration<QELayout>
    {
        public void Configure(EntityTypeBuilder<QELayout> builder)
        {
            builder.ToTable("tblQELayout");
            builder.HasIndex(qe => new { qe.QESetupID, qe.DataSourceID }).IsUnique(); //unique index
        }
    }
}



