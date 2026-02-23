using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TLTrademarkNameUpdateMap : IEntityTypeConfiguration<TLTrademarkNameUpdate>
    {
        public void Configure(EntityTypeBuilder<TLTrademarkNameUpdate> builder)
        {
            builder.HasNoKey().ToView("vwTLTrademarkNameUpdate");
            
        }
    }
}
