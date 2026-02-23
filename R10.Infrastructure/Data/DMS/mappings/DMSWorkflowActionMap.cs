using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSWorkflowActionMap : IEntityTypeConfiguration<DMSWorkflowAction>
    {
        public void Configure(EntityTypeBuilder<DMSWorkflowAction> builder)
        {
            builder.ToTable("tblDMSWorkflowAction");
            builder.HasIndex(c => new { c.WrkId, c.ActionTypeId, c.ActionValueId }).IsUnique();

        }
    }
}
