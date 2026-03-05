using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatActionTypeMap : IEntityTypeConfiguration<PatActionType>
    {
        public void Configure(EntityTypeBuilder<PatActionType> builder)
        {
            builder.ToTable("tblPatActionType");
            builder.HasIndex(a => new { a.ActionType, a.Country, a.CDueId }).IsUnique();
            builder.HasMany(a => a.ActionParameters).WithOne(p => p.ActionType);
            // builder.HasOne(a => a.Responsible).WithMany(r => r.AttorneyPatActionTypes).HasForeignKey(a => a.ResponsibleID).HasPrincipalKey(a => a.AttorneyID); // Removed: Responsible (Attorney) nav property no longer exists
            builder.HasOne(a => a.PatCountry).WithMany(c => c.PatActionTypes).HasForeignKey(a => a.Country).HasPrincipalKey(c=> c.Country);
            //builder.HasMany(a => a.PatCountryDues).WithOne(d => d.PatActionType).HasPrincipalKey(a => a.CDueId)
            //    .HasForeignKey(c => c.CDueId);
        }
    }
}
