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
    public class PatIRFRRemunerationMap : IEntityTypeConfiguration<PatIRFRRemuneration>
    {
        public void Configure(EntityTypeBuilder<PatIRFRRemuneration> builder)
        {
            builder.ToTable("tblPatIRFRRemuneration");
            builder.Property(c => c.FRRemunerationId).ValueGeneratedOnAdd();
            builder.Property(m => m.FRRemunerationId).UseIdentityColumn();
            builder.HasIndex(i => i.InvId).IsUnique();
        }
    }
}
