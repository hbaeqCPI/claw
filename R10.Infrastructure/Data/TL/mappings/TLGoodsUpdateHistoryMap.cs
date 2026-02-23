using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TLGoodsUpdateHistoryMap : IEntityTypeConfiguration<TLGoodsUpdateHistory>
    {
        public void Configure(EntityTypeBuilder<TLGoodsUpdateHistory> builder)
        {
            builder.HasNoKey().ToView("vwTLUpdLogGoods");
            
        }
    }
}
