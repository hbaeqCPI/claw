using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class RTSSearchActionMap : IEntityTypeConfiguration<RTSSearchAction>
    {
        public void Configure(EntityTypeBuilder<RTSSearchAction> builder)
        {
            builder.ToTable("tblPLSearchAction");

        }
    }
}
