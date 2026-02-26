using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatInventorMap : IEntityTypeConfiguration<PatInventor>
    {
        public void Configure(EntityTypeBuilder<PatInventor> builder)
        {
            builder.ToTable("tblPatInventor");
            builder.HasOne(i => i.AddressCountry).WithMany(pc => pc.CountryInventors).HasForeignKey(i => i.Country).HasPrincipalKey(pc => pc.Country);
            builder.HasOne(i => i.CitizenshipCountry).WithMany(pc => pc.CitizenshipInventors).HasForeignKey(i => i.Citizenship).HasPrincipalKey(pc => pc.Country);
            builder.HasOne(i => i.POAddressCountry).WithMany(pc => pc.POCountryInventors).HasForeignKey(i => i.POCountry).HasPrincipalKey(pc => pc.Country);
            builder.Property(p => p.Inventor)
                   .HasComputedColumnSql("Ltrim(IsNull([LastName], '') + Case When IsNull([LastName], '') <> '' And (IsNull([FirstName], '') <> '' Or IsNull([MiddleInitial], '') <> '') Then ',' Else '' End + Case When Isnull([FirstName],'') = '' Then '' Else ' ' + [FirstName] End + Case When Isnull([MiddleInitial],'') = '' Then '' Else ' ' + [MiddleInitial] End)");

            // Removed during deep clean
            // builder.HasMany(i => i.DisclosureInventors).WithOne(a => a.PatInventor).HasForeignKey(a => a.InventorID).HasPrincipalKey(i => i.InventorID);
            builder.HasMany(i => i.InventorAppAwards).WithOne(a => a.PatInventor).HasForeignKey(a => a.InventorID).HasPrincipalKey(i => i.InventorID);
            builder.HasMany(i => i.InventorDMSAwards).WithOne(a => a.PatInventor).HasForeignKey(a => a.InventorID).HasPrincipalKey(i => i.InventorID);
            builder.HasMany(i => i.InventorInventions).WithOne(a => a.InventorInvInventor).HasForeignKey(a => a.InventorID).HasPrincipalKey(i => i.InventorID);
            builder.HasMany(i => i.InventorCountryApplications).WithOne(a => a.InventorAppInventor).HasForeignKey(a => a.InventorID).HasPrincipalKey(i => i.InventorID);
            // Removed during deep clean
            // builder.HasMany(i => i.PacClearanceInventors).WithOne(a => a.PatInventor).HasForeignKey(a => a.InventorID).HasPrincipalKey(i => i.InventorID);
            builder.HasOne(i => i.Manager).WithMany(a => a.ManagerInventors).HasForeignKey(a => a.ManagerId).HasPrincipalKey(i => i.InventorID);
            builder.HasOne(i => i.Position).WithMany(a => a.Inventors).HasForeignKey(a => a.PositionId).HasPrincipalKey(i => i.PositionId);
            builder.HasMany(i => i.EntityFilters).WithOne(ef => ef.PatInventor).HasForeignKey(ef => ef.EntityId).HasPrincipalKey(i => i.InventorID);
        }

}


}
