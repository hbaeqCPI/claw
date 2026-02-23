using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;


namespace R10.Core.Interfaces
{
    
    public interface IPatCountryDueRepository 
    {
        Task GenerateCountryLawActions(CountryLawRetroParam criteria);
    }
}
