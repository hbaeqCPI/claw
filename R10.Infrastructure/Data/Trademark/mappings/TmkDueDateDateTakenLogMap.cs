using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDueDateDateTakenLogMap : IEntityTypeConfiguration<TmkDueDateDateTakenLog>
    {
        public void Configure(EntityTypeBuilder<TmkDueDateDateTakenLog> builder)
        {
            builder.ToTable("tblTmkDueDateTakenLog");
            //builder.HasOne(c => c.PatDueDate).WithMany(d => d.DueDateDeDockets).HasForeignKey(c => c.DDId).HasPrincipalKey(d=>d.DDId);
            builder.HasOne(a => a.TmkDueDate).WithMany(a => a.DateTakenLogs).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
