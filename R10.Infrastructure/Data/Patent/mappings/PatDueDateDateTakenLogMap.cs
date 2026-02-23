using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateDateTakenLogMap : IEntityTypeConfiguration<PatDueDateDateTakenLog>
    {
        public void Configure(EntityTypeBuilder<PatDueDateDateTakenLog> builder)
        {
            builder.ToTable("tblPatDueDateTakenLog");
            //builder.HasOne(c => c.PatDueDate).WithMany(d => d.DueDateDeDockets).HasForeignKey(c => c.DDId).HasPrincipalKey(d=>d.DDId);
            builder.HasOne(a => a.PatDueDate).WithMany(a => a.DateTakenLogs).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
