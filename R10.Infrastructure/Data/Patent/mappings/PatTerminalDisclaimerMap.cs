using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;


namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatTerminalDisclaimerMap : IEntityTypeConfiguration<PatTerminalDisclaimer>
    {
        public void Configure(EntityTypeBuilder<PatTerminalDisclaimer> builder)
        {
            builder.ToTable("tblPatTerminalDisclaimer");
            builder.HasOne(h => h.CountryApplication).WithMany(c => c.PatTerminalDisclaimers).HasForeignKey(h => h.AppId).HasPrincipalKey(c=>c.AppId);
            builder.HasOne(h => h.TerminalDiscCountryApplication).WithMany(c => c.PatChildTerminalDisclaimers).HasForeignKey(h => h.TerminalDisclaimerAppId).HasPrincipalKey(c => c.AppId);

        }
    }
}
