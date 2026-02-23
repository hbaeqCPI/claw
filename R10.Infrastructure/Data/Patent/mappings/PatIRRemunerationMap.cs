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
    public class PatIRRemunerationMap : IEntityTypeConfiguration<PatIRRemuneration>
    {
        public void Configure(EntityTypeBuilder<PatIRRemuneration> builder)
        {
            builder.ToTable("tblPatIRRemuneration");
            builder.Property(c => c.RemunerationId).ValueGeneratedOnAdd();
            builder.Property(m => m.RemunerationId).UseIdentityColumn();
            builder.HasIndex(i => i.InvId).IsUnique();
        }
    }
}
