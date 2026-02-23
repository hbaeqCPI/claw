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
    public class PatIRRemunerationTypeMap : IEntityTypeConfiguration<PatIRRemunerationType>
    {
        public void Configure(EntityTypeBuilder<PatIRRemunerationType> builder)
        {
            builder.ToTable("tblPatIRRemunerationType");
            builder.Property(c => c.RemunerationTypeId).ValueGeneratedOnAdd();
            builder.Property(m => m.RemunerationTypeId).UseIdentityColumn();
            builder.HasIndex(c => c.RemunerationType).IsUnique();
        }
    }
}
