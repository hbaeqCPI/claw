using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class LSDTextMap : IEntityTypeConfiguration<LSDText>
    {
        public void Configure(EntityTypeBuilder<LSDText> builder)
        {
            builder.ToTable("tblLSDText");
            builder.Property(m => m.LSDTextID).UseIdentityColumn();
        }
    }
}
