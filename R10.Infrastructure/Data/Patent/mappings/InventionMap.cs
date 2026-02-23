using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class InventionMap : IEntityTypeConfiguration<Invention>
    {
        public void Configure(EntityTypeBuilder<Invention> builder)
        {

            builder.ToTable("tblPatInvention");
            builder.HasKey("InvId");
            builder.Property(i => i.InvId).HasColumnName("InvId").IsRequired();
            builder.Property(i => i.CaseNumber).HasColumnName("CaseNumber").HasMaxLength(25);
            builder.HasIndex(i => i.CaseNumber).IsUnique();
            builder.HasMany(i => i.CountryApplications).WithOne(ca => ca.Invention).HasForeignKey(ca => ca.InvId).HasPrincipalKey(i=> i.InvId);

            builder.OwnsOne(i => i.TradeSecret, b => b.ToJson());
            builder.HasMany(i => i.TradeSecretRequests).WithOne(ts => ts.Invention).HasForeignKey(ts => ts.RecId).HasPrincipalKey(i => i.InvId);
        }
    }
}
