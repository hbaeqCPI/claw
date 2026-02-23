using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDueDateDeDocketOutstandingMap : IEntityTypeConfiguration<TmkDueDateDeDocketOutstanding>
    {
        public void Configure(EntityTypeBuilder<TmkDueDateDeDocketOutstanding> builder)
        {
            builder.ToTable("vwTmkDeDocketOutstanding");
            //builder.HasOne(dd => dd.TmkDueDate).WithOne(d => d.DeDocketOutstanding).HasForeignKey<TmkDueDate>();
        }
    }
}
