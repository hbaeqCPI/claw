using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateInvDateTakenLogMap : IEntityTypeConfiguration<PatDueDateInvDateTakenLog>
    {
        public void Configure(EntityTypeBuilder<PatDueDateInvDateTakenLog> builder)
        {
            builder.ToTable("tblPatDueDateInvTakenLog");
            //builder.HasOne(c => c.PatDueDate).WithMany(d => d.DueDateDeDockets).HasForeignKey(c => c.DDId).HasPrincipalKey(d=>d.DDId);
            builder.HasOne(a => a.PatDueDateInv).WithMany(a => a.DateTakenLogs).HasForeignKey(a => a.DDId).HasPrincipalKey(a => a.DDId);
        }
    }
}
