using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacClearanceMap : IEntityTypeConfiguration<PacClearance>
    {
        public void Configure(EntityTypeBuilder<PacClearance> builder)
        {
            builder.ToTable("tblPacClearance");
            builder.HasIndex(t => new { t.CaseNumber }).IsUnique();
            builder.Property(d => d.CaseNumber)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("('T-' + CONVERT([nvarchar],YEAR(GETDATE())) + CONVERT([nvarchar],ident_current('tblPacClearance')))");
        }
    }
}