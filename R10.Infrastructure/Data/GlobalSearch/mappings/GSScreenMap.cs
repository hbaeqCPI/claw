using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GlobalSearch;

namespace R10.Infrastructure.Data.GlobalSearch.mappings
{
    public class GSScreenMap : IEntityTypeConfiguration<GSScreen>
    {
        public void Configure(EntityTypeBuilder<GSScreen> builder)
        {
            builder.ToTable("tblGSScreen");
            builder.HasOne(d => d.GSSystem).WithMany(d => d.GSScreens).HasPrincipalKey(d => d.SystemType).HasForeignKey(d => d.SystemType);
        }
    }
}
