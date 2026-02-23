using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TmkDueDateDeDocketMap : IEntityTypeConfiguration<TmkDueDateDeDocket>
    {
        public void Configure(EntityTypeBuilder<TmkDueDateDeDocket> builder)
        {
            builder.ToTable("tblTmkDueDateDeDocket");
            builder.HasOne(c => c.TmkDueDate).WithMany(d => d.DueDateDeDockets).HasForeignKey(c => c.DDId).HasPrincipalKey(d => d.DDId);

        }
    }
}
