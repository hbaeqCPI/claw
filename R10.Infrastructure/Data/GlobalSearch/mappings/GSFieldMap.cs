using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GlobalSearch;

namespace R10.Infrastructure.Data.GlobalSearch.mappings
{
    public class GSFieldMap : IEntityTypeConfiguration<GSField>
    {
        public void Configure(EntityTypeBuilder<GSField> builder)
        {
            builder.ToTable("tblGSField");
            builder.HasOne(d => d.GSScreen).WithMany(d => d.GSFields).HasPrincipalKey(d => d.ScreenId).HasForeignKey(d => d.ScreenId);
            builder.HasOne(d => d.GSTable).WithMany(d => d.GSFields).HasPrincipalKey(d => d.TableId).HasForeignKey(d => d.TableId);
        }
    }
}
