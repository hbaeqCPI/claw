using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPortal.Core.DTOs;
using LawPortal.Core.Entities.Trademark;


namespace LawPortal.Core.Interfaces
{
    
    public interface ITmkCountryDueRepository 
    {
        Task GenerateCountryLawActions(CountryLawRetroParam criteria);

    }
}
