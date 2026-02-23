using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateMap : IEntityTypeConfiguration<PatDueDate>
    {
        public void Configure(EntityTypeBuilder<PatDueDate> builder)
        {
            builder.ToTable("tblPatDueDate");
            builder.HasIndex(a => new { a.ActId, a.ActionDue, a.DueDate }).IsUnique();
            builder.HasOne(a => a.PatActionDue).WithMany(a => a.DueDates).HasForeignKey(a => a.ActId).HasPrincipalKey(a => a.ActId);
            builder.HasOne(a => a.DeDocketOutstanding).WithOne(a => a.PatDueDate).HasForeignKey<PatDueDateDeDocketOutstanding>(a => a.DDId).HasPrincipalKey<PatDueDate>(a=>a.DDId);
        }
    }
}
