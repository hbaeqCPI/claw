using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatProductMap : IEntityTypeConfiguration<PatProduct>
    {
        public void Configure(EntityTypeBuilder<PatProduct> builder)
        {
            builder.ToTable("tblPatProduct");
            builder.HasOne(p => p.Application).WithMany(p => p.Products).HasForeignKey(l => l.AppId).HasPrincipalKey(c => c.AppId);

        }
    }
}
