using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMOtherPartyTypeMap : IEntityTypeConfiguration<GMOtherPartyType>
    {
        public void Configure(EntityTypeBuilder<GMOtherPartyType> builder)
        {
            builder.ToTable("tblGMOtherPartyType");
            builder.Property(e => e.TypeID).ValueGeneratedOnAdd();
            builder.Property(e => e.TypeID).UseIdentityColumn();
            builder.Property(e => e.TypeID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
