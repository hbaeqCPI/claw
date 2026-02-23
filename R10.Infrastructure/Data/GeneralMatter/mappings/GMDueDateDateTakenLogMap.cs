using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMDueDateDateTakenLogMap : IEntityTypeConfiguration<GMDueDateDateTakenLog>
    {
        public void Configure(EntityTypeBuilder<GMDueDateDateTakenLog> builder)
        {
            builder.ToTable("tblGMDueDateTakenLog");
            //builder.HasOne(c => c.PatDueDate).WithMany(d => d.DueDateDeDockets).HasForeignKey(c => c.DDId).HasPrincipalKey(d=>d.DDId);
            builder.HasOne(a => a.GMDueDate).WithMany(a => a.DateTakenLogs).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
