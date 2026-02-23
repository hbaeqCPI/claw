using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class CountryApplicationMap : IEntityTypeConfiguration<CountryApplication>
    {
        public void Configure(EntityTypeBuilder<CountryApplication> builder)
        {

            builder.ToTable("tblPatCountryApplication");
            builder.HasKey("AppId");
            builder.HasOne(ca => ca.ParentCase).WithMany(ca => ca.ChildCases).HasForeignKey(ca => ca.ParentAppId).HasPrincipalKey(ca => ca.AppId);
            //builder.HasOne(ca => ca.RelatedTerminalDisclaimer).WithMany(ca => ca.ChildTerminalDisclaimers).HasForeignKey(ca => ca.TerminalDisclaimerAppId).HasPrincipalKey(ca => ca.AppId);
            builder.HasOne(ca => ca.IDSRelatedCasesInfo).WithOne(i => i.CountryApplication).HasForeignKey<PatIDSRelatedCasesInfo>(i=> i.AppId);
            builder.OwnsOne(ca => ca.TradeSecret, b => b.ToJson());
        }
    }
}
