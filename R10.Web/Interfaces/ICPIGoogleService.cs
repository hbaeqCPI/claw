using R10.Core;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Web.Areas.Patent.ViewModels;
using R10.Core.Entities;
using R10.Core.DTOs;

namespace R10.Web.Interfaces
{
    public interface ICPIGoogleService
    {
        Task<int> GetIDSDocuments(int logId, PatIDSSearchApi? patIDSSearchApi = null);
        Task<int> UpdateCurrencyExRates();
    }
}
