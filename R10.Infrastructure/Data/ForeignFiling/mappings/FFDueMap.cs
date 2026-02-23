using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFDueMap : IEntityTypeConfiguration<FFDue>
    {
        public void Configure(EntityTypeBuilder<FFDue> builder)
        {
            builder.ToTable("tblFFDue");
            builder.HasKey(d => d.DueId);
            builder.HasIndex(d => d.DDId).IsUnique();
            builder.HasOne(d => d.PatDueDate).WithOne(t => t.FFDue).HasForeignKey<FFDue>(t => t.DDId).HasPrincipalKey<PatDueDate>(d => d.DDId);
        }
    }
}
