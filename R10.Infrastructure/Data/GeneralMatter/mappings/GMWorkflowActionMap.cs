using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;


namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMWorkflowActionMap : IEntityTypeConfiguration<GMWorkflowAction>
    {
        public void Configure(EntityTypeBuilder<GMWorkflowAction> builder)
        {
            builder.ToTable("tblGMWorkflowAction");
            builder.HasIndex(c => new { c.WrkId, c.ActionTypeId, c.ActionValueId }).IsUnique();
        }
    }
}
