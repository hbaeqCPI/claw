using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatSearchFieldMap : IEntityTypeConfiguration<PatSearchField>
    {
        public void Configure(EntityTypeBuilder<PatSearchField> builder)
        {
            builder.ToTable("tblPatSearchField");
        }
    }

    public class PatSearchNotifyMap : IEntityTypeConfiguration<PatSearchNotify>
    {
        public void Configure(EntityTypeBuilder<PatSearchNotify> builder)
        {
            builder.ToTable("tblPatSearchNotify");
            builder.HasOne(n => n.QEMain).WithMany(q => q.PatSearchsNotify).HasForeignKey(n => n.QESetupId).HasPrincipalKey(q => q.QESetupID);
        }
    }

    public class PatSearchNotifyLogMap : IEntityTypeConfiguration<PatSearchNotifyLog>
    {
        public void Configure(EntityTypeBuilder<PatSearchNotifyLog> builder)
        {
            builder.ToTable("tblPatSearchNotifyLog");
        }
    }
}
