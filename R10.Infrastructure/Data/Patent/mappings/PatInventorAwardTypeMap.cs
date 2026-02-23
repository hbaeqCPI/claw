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
    public class PatInventorAwardTypeMap : IEntityTypeConfiguration<PatInventorAwardType>
    {
        public void Configure(EntityTypeBuilder<PatInventorAwardType> builder)
        {
            builder.ToTable("tblPatInventorAwardType");
            builder.Property(s => s.AwardTypeId).ValueGeneratedOnAdd();
            builder.Property(m => m.AwardTypeId).UseIdentityColumn();
            builder.HasIndex(s => new { s.AwardType}).IsUnique();
            
        }
    }
}
