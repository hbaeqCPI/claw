using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcClearanceMap : IEntityTypeConfiguration<TmcClearance>
    {
        public void Configure(EntityTypeBuilder<TmcClearance> builder)
        {
            builder.ToTable("tblTmcClearance");
            builder.HasIndex(t => new { t.CaseNumber }).IsUnique();
            builder.Property(d => d.CaseNumber)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("('T-' + CONVERT([nvarchar],YEAR(GETDATE())) + CONVERT([nvarchar],ident_current('tblTmcClearance')))");
        }
    }
}