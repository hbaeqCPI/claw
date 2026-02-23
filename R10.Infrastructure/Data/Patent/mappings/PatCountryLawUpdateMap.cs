using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryLawUpdateMap : IEntityTypeConfiguration<PatCountryLawUpdate>
    {
        public void Configure(EntityTypeBuilder<PatCountryLawUpdate> builder)
        {
            builder.ToTable("tblPatCountryLawUpdate");                        
            builder.HasIndex(s => new { s.Year, s.Quarter}).IsUnique();
            builder.Property(s => s.keyID).UseIdentityColumn();
        }
    }
}
