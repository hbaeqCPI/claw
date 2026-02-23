using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ContactPersonMap : IEntityTypeConfiguration<ContactPerson>
    {
        public void Configure(EntityTypeBuilder<ContactPerson> builder)
        {

            builder.ToTable("tblContactPerson");
            builder.HasIndex(cp => cp.Contact).IsUnique();
            builder.Property(c => c.ContactName).HasComputedColumnSql("[ContactName]");
            builder.HasOne(cp => cp.ContactLanguage).WithMany(l => l.LanguageContacts).HasForeignKey(cp => cp.Language).HasPrincipalKey(l => l.LanguageName).IsRequired(false);
            builder.HasOne(cp => cp.AddressCountry).WithMany(c => c.CountryContactPersons).HasForeignKey(c => c.Country).HasPrincipalKey(c => c.Country);
            builder.HasMany(cp => cp.EntityFilters).WithOne(ef => ef.ContactPerson).HasForeignKey(ef => ef.EntityId).HasPrincipalKey(cp => cp.ContactID);
        }
    }
}
