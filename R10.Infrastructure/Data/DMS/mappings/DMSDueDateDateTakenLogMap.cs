using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSDueDateDateTakenLogMap : IEntityTypeConfiguration<DMSDueDateDateTakenLog>
    {
        public void Configure(EntityTypeBuilder<DMSDueDateDateTakenLog> builder)
        {
            builder.ToTable("tblDMSDueDateTakenLog");
            builder.HasOne(a => a.DMSDueDate).WithMany(a => a.DateTakenLogs).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
