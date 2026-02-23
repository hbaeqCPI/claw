using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkOwnerMap : IEntityTypeConfiguration<TmkOwner>
    {
        public void Configure(EntityTypeBuilder<TmkOwner> builder)
        {
            builder.ToTable("tblTmkOwner");
            builder.HasOne(to => to.TmkTrademark).WithMany(t => t.Owners).HasForeignKey(to => to.TmkID);
        }
    }
}
