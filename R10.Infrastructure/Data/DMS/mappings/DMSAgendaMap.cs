using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSAgendaMap: IEntityTypeConfiguration<DMSAgenda>
    {
        public void Configure(EntityTypeBuilder<DMSAgenda> builder)
        {
            builder.ToTable("tblDMSAgenda");
            builder.HasKey(a => a.AgendaId);
            builder.HasIndex(a => new { a.MeetingDate, a.ClientID }).IsUnique();
        }
    }
}
