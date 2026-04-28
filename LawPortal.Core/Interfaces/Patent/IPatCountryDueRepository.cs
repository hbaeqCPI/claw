using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using LawPortal.Core.DTOs;
using LawPortal.Core.Entities.Patent;


namespace LawPortal.Core.Interfaces
{
    
    public interface IPatCountryDueRepository 
    {
        Task GenerateCountryLawActions(CountryLawRetroParam criteria);
    }
}
