using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFActionCloseLogEmailMap : IEntityTypeConfiguration<FFActionCloseLogEmail>
    {
        public void Configure(EntityTypeBuilder<FFActionCloseLogEmail> builder)
        {
            builder.ToTable("tblFFActionCloseLogEmail");
            builder.HasKey(e => e.LogEmailId);
            builder.HasOne(e => e.FFActionCloseLog).WithMany(l => l.FFActionCloseLogEmails).HasForeignKey(e => e.LogId).HasPrincipalKey(l => l.LogId);
        }
    }
}
