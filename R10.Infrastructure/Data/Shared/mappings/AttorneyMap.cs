using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class AttorneyMap : IEntityTypeConfiguration<Attorney>
    {
        public void Configure(EntityTypeBuilder<Attorney> builder)
        {

            builder.ToTable("tblAttorney");
            builder.Property(a => a.AttorneyCode).HasColumnName("Attorney");
            builder.HasIndex(a => a.AttorneyCode).IsUnique();
            builder.HasOne(a => a.AddressCountry).WithMany(pc => pc.CountryAttorneys).HasForeignKey(a => a.Country).HasPrincipalKey(pc => pc.Country);
            builder.HasOne(a => a.POAddressCountry).WithMany(pc => pc.POCountryAttorneys).HasForeignKey(a => a.POCountry).HasPrincipalKey(pc => pc.Country);

            builder.HasMany(a => a.PatDefaultAtty1Clients).WithOne(c => c.PatDefaultAtty1).HasForeignKey(c => c.PatAttorney1ID);
            builder.HasMany(a => a.PatDefaultAtty2Clients).WithOne(c => c.PatDefaultAtty2).HasForeignKey(c => c.PatAttorney2ID);
            builder.HasMany(a => a.PatDefaultAtty3Clients).WithOne(c => c.PatDefaultAtty3).HasForeignKey(c => c.PatAttorney3ID);
            builder.HasMany(a => a.PatDefaultAtty4Clients).WithOne(c => c.PatDefaultAtty4).HasForeignKey(c => c.PatAttorney4ID);
            builder.HasMany(a => a.PatDefaultAtty5Clients).WithOne(c => c.PatDefaultAtty5).HasForeignKey(c => c.PatAttorney5ID);

            builder.HasMany(a => a.TmkDefaultAtty1Clients).WithOne(c => c.TmkDefaultAtty1).HasForeignKey(c => c.TmkAttorney1ID);
            builder.HasMany(a => a.TmkDefaultAtty2Clients).WithOne(c => c.TmkDefaultAtty2).HasForeignKey(c => c.TmkAttorney2ID);
            builder.HasMany(a => a.TmkDefaultAtty3Clients).WithOne(c => c.TmkDefaultAtty3).HasForeignKey(c => c.TmkAttorney3ID);
            builder.HasMany(a => a.TmkDefaultAtty4Clients).WithOne(c => c.TmkDefaultAtty4).HasForeignKey(c => c.TmkAttorney4ID);
            builder.HasMany(a => a.TmkDefaultAtty5Clients).WithOne(c => c.TmkDefaultAtty5).HasForeignKey(c => c.TmkAttorney5ID);

            builder.HasMany(a => a.Attorney1Inventions).WithOne(i => i.Attorney1).HasForeignKey(i => i.Attorney1ID);
            builder.HasMany(a => a.Attorney2Inventions).WithOne(i => i.Attorney2).HasForeignKey(i => i.Attorney2ID);
            builder.HasMany(a => a.Attorney3Inventions).WithOne(i => i.Attorney3).HasForeignKey(i => i.Attorney3ID);
            builder.HasMany(a => a.Attorney4Inventions).WithOne(i => i.Attorney4).HasForeignKey(i => i.Attorney4ID);
            builder.HasMany(a => a.Attorney5Inventions).WithOne(i => i.Attorney5).HasForeignKey(i => i.Attorney5ID);

            builder.HasMany(a => a.AttorneyDisclosures).WithOne(d => d.Attorney).HasForeignKey(d => d.AttorneyID);

            builder.HasMany(a => a.Attorney1Trademarks).WithOne(t => t.Attorney1).HasForeignKey(t => t.Attorney1ID);
            builder.HasMany(a => a.Attorney2Trademarks).WithOne(t => t.Attorney2).HasForeignKey(t => t.Attorney2ID);
            builder.HasMany(a => a.Attorney3Trademarks).WithOne(t => t.Attorney3).HasForeignKey(t => t.Attorney3ID);
            builder.HasMany(a => a.Attorney4Trademarks).WithOne(t => t.Attorney4).HasForeignKey(t => t.Attorney4ID);
            builder.HasMany(a => a.Attorney5Trademarks).WithOne(t => t.Attorney5).HasForeignKey(t => t.Attorney5ID);

            builder.HasMany(a => a.PatCostTrackBillings).WithOne(t => t.BillingAttorney).HasForeignKey(t => t.BillingAttorneyId);

            builder.HasMany(a => a.TmkCostTrackBillings).WithOne(t => t.BillingAttorney).HasForeignKey(t => t.BillingAttorneyId);

            builder.HasMany(a => a.GMCostTrackBillings).WithOne(t => t.BillingAttorney).HasForeignKey(t => t.BillingAttorneyId);

            builder.HasOne(o => o.AttorneyLanguage).WithMany(m => m.LanguageAttorneys).HasForeignKey(f => f.Language).HasPrincipalKey(k => k.LanguageName);

            builder.HasMany(a => a.AttorneyDMSDueDates).WithOne(d => d.DueDateAttorney).IsRequired(false).HasForeignKey(d => d.AttorneyID);
            builder.HasMany(a => a.AttorneyGMDueDates).WithOne(d => d.DueDateAttorney).IsRequired(false).HasForeignKey(d => d.AttorneyID);
            builder.HasMany(a => a.AttorneyPatDueDates).WithOne(d => d.DueDateAttorney).IsRequired(false).HasForeignKey(d => d.AttorneyID);
            builder.HasMany(a => a.AttorneyTmkDueDates).WithOne(d => d.DueDateAttorney).IsRequired(false).HasForeignKey(d => d.AttorneyID);

            builder.HasMany(a => a.EntityFilters).WithOne(ef => ef.Attorney).HasForeignKey(ef => ef.EntityId).HasPrincipalKey(a => a.AttorneyID);
        }
    }
}
