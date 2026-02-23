using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXUserDataMap : IEntityTypeConfiguration<DOCXUserData>
    {
        public void Configure(EntityTypeBuilder<DOCXUserData> builder)
        {
            builder.ToTable("tblDOCXUserData");
            builder.HasIndex(d => new {d.DOCXId, d.DataName}).IsUnique();
            builder.HasOne(d => d.DOCXMain).WithMany(l => l.DOCXUserData).HasForeignKey(d=>d.DOCXId).HasPrincipalKey(l=>l.DOCXId);
        }
    }
}
