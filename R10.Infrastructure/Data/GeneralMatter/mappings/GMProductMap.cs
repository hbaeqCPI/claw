using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMProductMap : IEntityTypeConfiguration<GMProduct>
    {
        public void Configure(EntityTypeBuilder<GMProduct> builder)
        {
            builder.ToTable("tblGMProduct");
            builder.HasOne(t => t.Matter).WithMany(t => t.GMProducts).HasForeignKey(l => l.MatId).HasPrincipalKey(c => c.MatId);

        }
    }
}
