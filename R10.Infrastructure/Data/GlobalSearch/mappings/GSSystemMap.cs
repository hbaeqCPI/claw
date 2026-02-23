using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GlobalSearch;

namespace R10.Infrastructure.Data.GlobalSearch.mappings
{
    public class GSSystemMap : IEntityTypeConfiguration<GSSystem>
    {
        public void Configure(EntityTypeBuilder<GSSystem> builder)
        {
            builder.ToTable("tblGSSystem");
            builder.HasOne(c => c.CPiSystem).WithMany(s => s.GSSystems).HasForeignKey(c => c.SystemType).HasPrincipalKey(s => s.SystemType);
        }
    }
}
