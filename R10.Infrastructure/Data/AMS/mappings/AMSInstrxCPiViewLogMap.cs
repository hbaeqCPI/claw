using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSInstrxCPiViewLogMap : IEntityTypeConfiguration<AMSInstrxCPiViewLog>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxCPiViewLog> builder)
        {
            builder.ToTable("tblAMSInstrxCPiViewLog");
            builder.HasKey(d => d.Id);
            builder.HasIndex(d => d.UserName).IsUnique();
        }
    }
}
