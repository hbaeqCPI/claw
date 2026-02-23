using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class GMDueDateDeDocketMap : IEntityTypeConfiguration<GMDueDateDeDocket>
    {
        public void Configure(EntityTypeBuilder<GMDueDateDeDocket> builder)
        {
            builder.ToTable("tblGMDueDateDeDocket");
            builder.HasOne(c => c.GMDueDate).WithMany(d => d.DueDateDeDockets).HasForeignKey(c => c.DDId).HasPrincipalKey(d => d.DDId);

        }
    }
}
