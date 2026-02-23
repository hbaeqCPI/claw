using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSValuationMatrixMap : IEntityTypeConfiguration<DMSValuationMatrix>
    {
        public void Configure(EntityTypeBuilder<DMSValuationMatrix> builder)
        {
            builder.ToTable("tblDMSValuationMatrix");
            builder.HasIndex(val => new { val.Category }).IsUnique();
            builder.HasMany(val => val.DMSValuationMatrixRates).WithOne(d => d.DMSValuationMatrix).HasPrincipalKey(c => c.ValId).HasForeignKey(d => d.ValId);
        }
    }
}
