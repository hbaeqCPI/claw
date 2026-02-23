using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GlobalSearch;

namespace R10.Infrastructure.Data.GlobalSearch.mappings
{
    public class GSTableMap : IEntityTypeConfiguration<GSTable>
    {
        public void Configure(EntityTypeBuilder<GSTable> builder)
        {
            builder.ToTable("tblGSTable");
        }
    }
}
