using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class QETagMap : IEntityTypeConfiguration<QETag>
    {
        public void Configure(EntityTypeBuilder<QETag> builder)
        {
            builder.ToTable("tblQETag");
            builder.Property(d => d.QETagId).ValueGeneratedOnAdd();
            builder.HasOne(d => d.QE).WithMany(f => f.QETags).HasForeignKey(d => d.QESetupId).HasPrincipalKey(d => d.QESetupID);
        }
    }
}
