using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSDueMap : IEntityTypeConfiguration<RMSDue>
    {
        public void Configure(EntityTypeBuilder<RMSDue> builder)
        {
            builder.ToTable("tblRMSDue");
            builder.HasKey(d => d.DueId);
            builder.HasIndex(d => d.DDId).IsUnique();
            builder.HasOne(d => d.TmkDueDate).WithOne(t => t.RMSDue).HasForeignKey<RMSDue>(t => t.DDId).HasPrincipalKey<TmkDueDate>(d => d.DDId);
        }
    }
}
