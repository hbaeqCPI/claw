using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPOMailLogMap : IEntityTypeConfiguration<PatEPOMailLog>
    {
        public void Configure(EntityTypeBuilder<PatEPOMailLog> builder)
        {

            builder.ToTable("tblPatEPOMailLog");
            builder.Property(s => s.KeyId).ValueGeneratedOnAdd();
            builder.Property(m => m.KeyId).UseIdentityColumn();
            builder.HasOne(vd => vd.EPOCommunication).WithMany(vd => vd.PatEPOMailLogs).HasForeignKey(vd => vd.CommunicationId).HasPrincipalKey(pk => pk.CommunicationId).IsRequired(false);
        }
    }
}