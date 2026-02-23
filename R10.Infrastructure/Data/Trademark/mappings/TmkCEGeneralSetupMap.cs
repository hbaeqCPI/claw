using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCEGeneralSetupMap : IEntityTypeConfiguration<TmkCEGeneralSetup>
    {
        public void Configure(EntityTypeBuilder<TmkCEGeneralSetup> builder)
        {
            builder.ToTable("tblTmkCEGeneralSetup");
            builder.HasIndex(c => new { c.CostSetup }).IsUnique();
            builder.HasMany(f => f.Client).WithOne(c => c.TmkCEGeneralSetup).HasForeignKey(c => c.TmkCEGeneralId).HasPrincipalKey(f => f.CEGeneralId);
        }
    }
}
