using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDisclosureStatusMap : IEntityTypeConfiguration<PatDisclosureStatus>
    {
        public void Configure(EntityTypeBuilder<PatDisclosureStatus> builder)
        {

            builder.ToTable("tblPatDisclosureStatus");                        
            builder.HasIndex(s => s.DisclosureStatus).IsUnique();
            builder.Property(s => s.DisclosureStatusID).ValueGeneratedOnAdd();
            builder.Property(s => s.DisclosureStatusID).UseIdentityColumn();
            builder.Property(s => s.DisclosureStatusID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
