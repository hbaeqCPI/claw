using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class AgentContactMap : IEntityTypeConfiguration<AgentContact>
    {
        public void Configure(EntityTypeBuilder<AgentContact> builder)
        {
            builder.ToTable("tblAgentContact");
            builder.HasIndex(ac => new { ac.AgentID, ac.ContactID }).IsUnique();

        }
    }
}
