using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFActionCloseLogErrorMap : IEntityTypeConfiguration<FFActionCloseLogError>
    {
        public void Configure(EntityTypeBuilder<FFActionCloseLogError> builder)
        {
            builder.ToTable("tblFFActionCloseLogError");
            builder.HasKey(e => e.LogErrorId);
            builder.HasOne(e => e.FFActionCloseLog).WithMany(l => l.FFActionCloseLogErrors).HasForeignKey(e => e.LogId).HasPrincipalKey(l => l.LogId);
        }
    }
}
