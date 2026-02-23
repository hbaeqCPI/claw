using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiGroupMap : IEntityTypeConfiguration<CPiGroup>
    {
        public void Configure(EntityTypeBuilder<CPiGroup> builder)
        {
            builder.ToTable("tblCPiGroups");
            builder.HasKey(x => new { x.Id });
        }
    }
}
