using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkProductMap : IEntityTypeConfiguration<TmkProduct>
    {
        public void Configure(EntityTypeBuilder<TmkProduct> builder)
        {
            builder.ToTable("tblTmkProduct");
            builder.HasOne(p => p.Trademark).WithMany(t => t.TmkProducts).HasForeignKey(p => p.TmkId).HasPrincipalKey(t => t.TmkId);

        }
    }
}
