using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class RTSSearchMap : IEntityTypeConfiguration<RTSSearch>
    {
        public void Configure(EntityTypeBuilder<RTSSearch> builder)
        {
            builder.ToTable("tblPLSearch");
            builder.HasMany(s => s.RTSSearchActions).WithOne(a => a.RTSSearch).HasForeignKey(a => a.PLAppId).HasPrincipalKey(s => s.PLAppId); 
            builder.HasOne(s => s.CountryApplication).WithOne(ca => ca.RTSSearch);
        }
    }
}
