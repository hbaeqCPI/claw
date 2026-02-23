using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.RMS;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSRemLogMap : IEntityTypeConfiguration<RemLog<TmkDueDate, RMSRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLog<TmkDueDate, RMSRemLogDue>> builder)
        {
            builder.ToTable("tblRMSRemLog");
            builder.HasKey(r => r.RemId);
        }
    }
}