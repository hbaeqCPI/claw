using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;


namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatWorkflowActionMap : IEntityTypeConfiguration<PatWorkflowAction>
    {
        public void Configure(EntityTypeBuilder<PatWorkflowAction> builder)
        {
            builder.ToTable("tblPatWorkflowAction");
            builder.HasIndex(c => new { c.WrkId, c.ActionTypeId, c.ActionValueId }).IsUnique();
        }
    }
}
