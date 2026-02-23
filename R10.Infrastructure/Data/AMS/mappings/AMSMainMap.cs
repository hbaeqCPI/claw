using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSMainMap : IEntityTypeConfiguration<AMSMain>
    {
        public void Configure(EntityTypeBuilder<AMSMain> builder)
        {
            builder.ToTable("tblAMSMain");
            builder.HasKey(m => m.AnnID);
            builder.HasIndex(m => new { m.CPIClientCode, m.CaseNumber, m.Country, m.SubCase, m.PrevCaseNumber }).IsUnique();
            builder.HasIndex(m => m.CPIClientCode).IsUnique(false);
            builder.HasIndex(m => m.CPIClient).IsUnique(false);
            builder.HasIndex(m => m.AS4AppID).IsUnique();
            builder.HasMany(m => m.AMSDue).WithOne(d => d.AMSMain).HasForeignKey(d => d.AnnID).HasPrincipalKey(m => m.AnnID);
            builder.HasMany(m => m.AMSProjection).WithOne(d => d.AMSMain).HasForeignKey(d => d.AnnID).HasPrincipalKey(m => m.AnnID);
            builder.HasOne(m => m.PatApplicationStatus).WithMany(s => s.AMSMain).HasForeignKey(m => m.CPIStatus).HasPrincipalKey(s => s.ApplicationStatus);
            builder.HasOne(m => m.Client).WithMany(c => c.ClientAMSMain).HasForeignKey(m => m.CPIClient).HasPrincipalKey(c => c.ClientCode).IsRequired(false);
            builder.HasOne(m => m.Attorney).WithMany(a => a.AttorneyAMSMain).HasForeignKey(m => m.CPIAttorney).HasPrincipalKey(a => a.AttorneyCode).IsRequired(false);
            builder.HasOne(m => m.Agent).WithMany(a => a.AgentAMSMain).HasForeignKey(m => m.CPIAgent).HasPrincipalKey(a => a.AgentCode).IsRequired(false);
            builder.HasOne(m => m.AMSAbstract).WithOne(a => a.AMSMain).HasForeignKey<AMSAbstract>(a => a.AnnID).HasPrincipalKey<AMSMain>(m => m.AnnID);
            builder.HasOne(m => m.PatCountry).WithMany(c => c.AMSMain).HasForeignKey(m => m.Country).HasPrincipalKey(c => c.Country).IsRequired(false);
            builder.HasOne(m => m.CountryApplication).WithOne(c => c.AMSMain)
                .HasForeignKey<CountryApplication>(c => new { c.CaseNumber, c.Country, c.SubCase })
                .HasPrincipalKey<AMSMain>(m => new { m.CaseNumber, m.Country, m.SubCase });
        }
    }
}
