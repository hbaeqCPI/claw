using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class EPOCommunicationMap : IEntityTypeConfiguration<EPOCommunication>
    {
        public void Configure(EntityTypeBuilder<EPOCommunication> builder)
        {
            builder.ToTable("tblEPOCommunication");
            builder.HasIndex(a => new { a.CommunicationId }).IsUnique();
        }
    }
}
