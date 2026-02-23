using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMAgreementTypeMap : IEntityTypeConfiguration<GMAgreementType>
    {
        public void Configure(EntityTypeBuilder<GMAgreementType> builder)
        {
            builder.ToTable("tblGMAgreementType");
            builder.Property(e => e.AgreementTypeID).ValueGeneratedOnAdd();
            builder.Property(e => e.AgreementTypeID).UseIdentityColumn();
            builder.Property(e => e.AgreementTypeID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
