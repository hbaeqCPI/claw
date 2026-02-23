using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ClientDesignatedCountryMap: IEntityTypeConfiguration<ClientDesignatedCountry>
    {
        public void Configure(EntityTypeBuilder<ClientDesignatedCountry> builder ) {

            builder.ToTable("tblClient_ExtDesCtry");
            builder.HasOne(d => d.Client).WithMany(c => c.ClientDesignatedCountries);
            
        }
    }
}
