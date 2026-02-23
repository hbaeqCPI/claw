using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPOCommActLogMap : IEntityTypeConfiguration<PatEPOCommActLog>
    {
        public void Configure(EntityTypeBuilder<PatEPOCommActLog> builder)
        {

            builder.ToTable("tblPatEPOCommActLog");
            builder.Property(s => s.ActLogId).ValueGeneratedOnAdd();
            builder.Property(m => m.ActLogId).UseIdentityColumn();
            builder.HasIndex(a => new { a.CommunicationId, a.ActId }).IsUnique();
        }
    }
}