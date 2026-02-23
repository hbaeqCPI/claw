using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class OwnerContactMap : IEntityTypeConfiguration<OwnerContact>
    {
        public void Configure(EntityTypeBuilder<OwnerContact> builder)
        {
            builder.ToTable("tblOwnerContact");
            builder.HasIndex(oc => new { oc.OwnerID, oc.ContactID }).IsUnique();

        }
    }
}
