using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Data.Shared.mappings
{
    public class CurrencyTypeMap : IEntityTypeConfiguration<CurrencyType>
    {
        public void Configure(EntityTypeBuilder<CurrencyType> builder)
        {
            builder.ToTable("tblCurrencyType");
            builder.Property(c => c.KeyID).ValueGeneratedOnAdd();
            builder.Property(c => c.KeyID).UseIdentityColumn();
            builder.Property(c => c.KeyID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.Property(c => c.CurrencyTypeCode).HasColumnName("CurrencyType");
            // builder.HasMany(o => o.AMSProjections).WithOne(m => m.CurrencyType).HasForeignKey(f => f.InvCurrency).HasPrincipalKey(k => k.CurrencyTypeCode);
        }
    }
}

