using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;


namespace R10.Core.Interfaces
{
    
    public interface ITmkCountryDueRepository 
    {
        Task GenerateCountryLawActions(CountryLawRetroParam criteria);

    }
}
