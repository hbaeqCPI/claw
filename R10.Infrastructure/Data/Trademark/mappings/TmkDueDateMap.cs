using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;


namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDueDateMap : IEntityTypeConfiguration<TmkDueDate>
    {
        public void Configure(EntityTypeBuilder<TmkDueDate> builder)
        {
            builder.ToTable("tblTmkDueDate");
            builder.HasIndex(d => new { d.ActId, d.ActionDue, d.DueDate }).IsUnique();
            builder.HasOne(d => d.TmkActionDue).WithMany(a => a.DueDates).HasForeignKey(d => d.ActId).HasPrincipalKey(a => a.ActId);
            builder.HasOne(a => a.DeDocketOutstanding).WithOne(a => a.TmkDueDate).HasForeignKey<TmkDueDateDeDocketOutstanding>(a => a.DDId).HasPrincipalKey<TmkDueDate>(a => a.DDId);
        }
    }
}
