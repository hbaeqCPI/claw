using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DisclosureMap : IEntityTypeConfiguration<Disclosure>
    {
        public void Configure(EntityTypeBuilder<Disclosure> builder)
        {

            builder.ToTable("tblDMSDisclosure");
            builder.HasKey("DMSId");
            builder.HasIndex(d => d.DisclosureNumber).IsUnique();
            builder.Property(d => d.DisclosureNumber)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("(('D'+CONVERT([nvarchar],ident_current('tblDMSDisclosure')))+'-00')");

            builder.OwnsOne(i => i.TradeSecret, b => b.ToJson());
            builder.HasMany(i => i.TradeSecretRequests).WithOne(ts => ts.Disclosure).HasForeignKey(ts => ts.RecId).HasPrincipalKey(i => i.DMSId);
        }
    }
}
