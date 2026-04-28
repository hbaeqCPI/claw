using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data
{
    public class ApiLogMap : IEntityTypeConfiguration<ApiLog>
    {
        public void Configure(EntityTypeBuilder<ApiLog> builder)
        {
            builder.ToTable("ApiLogs");
        }
    }
}
