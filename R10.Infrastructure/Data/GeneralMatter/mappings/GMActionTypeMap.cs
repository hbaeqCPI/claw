using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMActionTypeMap : IEntityTypeConfiguration<GMActionType>
    {
        public void Configure(EntityTypeBuilder<GMActionType> builder)
        {
            builder.ToTable("tblGMActionType");
            builder.HasIndex(a => new { a.ActionType, a.Country }).IsUnique();
            builder.HasMany(a => a.ActionParameters).WithOne(p => p.ActionType);
            builder.HasOne(a => a.Responsible).WithMany(r => r.AttorneyGMActionTypes).HasForeignKey(a => a.ResponsibleID).HasPrincipalKey(a => a.AttorneyID);
            builder.HasOne(a => a.GMCountry).WithMany(c => c.GMActionTypes).HasForeignKey(a => a.Country).HasPrincipalKey(a => a.Country);
        }
    }
}
