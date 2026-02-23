using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMAreaMap : IEntityTypeConfiguration<GMArea>
    {
        public void Configure(EntityTypeBuilder<GMArea> builder)
        {
            builder.ToTable("tblGMArea");
            builder.HasIndex(a => a.Area).IsUnique();
            builder.HasMany(a => a.GMAreaCountries).WithOne(ac => ac.GMArea).HasPrincipalKey(a => a.AreaID).HasForeignKey(ac => ac.AreaID);
        }
    }
}
