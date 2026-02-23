using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCountryLawUpdateMap : IEntityTypeConfiguration<TmkCountryLawUpdate>
    {
        public void Configure(EntityTypeBuilder<TmkCountryLawUpdate> builder)
        {
            builder.ToTable("tblTmkCountryLawUpdate");                        
            builder.HasIndex(s => new { s.Year, s.Quarter}).IsUnique();
            builder.Property(s => s.keyID).UseIdentityColumn();
        }
    }
}
