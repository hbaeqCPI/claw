using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatNonPatLiteratureMap : IEntityTypeConfiguration<PatIDSNonPatLiterature>
    {
        public void Configure(EntityTypeBuilder<PatIDSNonPatLiterature> builder)
        {
            builder.ToTable("tblPatAppNonPatLiterature");
            builder.HasOne(h => h.CountryApplication).WithMany(c=>c.NonPatLiteratures).HasForeignKey(r => r.AppId).HasPrincipalKey(c => c.AppId);

        }
    }
}
