using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class CountryApplicationWebSvcMap : IEntityTypeConfiguration<CountryApplicationWebSvc>
    {
        public void Configure(EntityTypeBuilder<CountryApplicationWebSvc> builder)
        {

            builder.ToTable("tblPatCountryApplicationWebSvc");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn();                       
        }
    }
}