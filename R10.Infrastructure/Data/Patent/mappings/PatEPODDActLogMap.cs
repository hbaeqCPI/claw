using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPODDActLogMap : IEntityTypeConfiguration<PatEPODDActLog>
    {
        public void Configure(EntityTypeBuilder<PatEPODDActLog> builder)
        {

            builder.ToTable("tblPatEPODDActLog");
            builder.Property(s => s.ActLogId).ValueGeneratedOnAdd();
            builder.Property(m => m.ActLogId).UseIdentityColumn();
            builder.HasIndex(a => new { a.EPODDId, a.ActId }).IsUnique();
        }
    }
}