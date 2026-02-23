using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSInventorHistoryMap : IEntityTypeConfiguration<DMSInventorHistory>
    {
        public void Configure(EntityTypeBuilder<DMSInventorHistory> builder)
        {

            builder.ToTable("tblDMSInventorHistory");
        }
    }
}
