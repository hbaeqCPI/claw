using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCEGeneralSetupMap : IEntityTypeConfiguration<PatCEGeneralSetup>
    {
        public void Configure(EntityTypeBuilder<PatCEGeneralSetup> builder)
        {
            builder.ToTable("tblPatCEGeneralSetup");
            builder.HasIndex(c => new { c.CostSetup }).IsUnique();
            builder.HasMany(f => f.Client).WithOne(c => c.PatCEGeneralSetup).HasForeignKey(c => c.PatCEGeneralId).HasPrincipalKey(f => f.CEGeneralId);
        }
    }
}
