using R10.Core.DTOs;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IFormExtractViewModelService
    {
        #region Search
        Task<List<LookupDTO>> GetSystemList(List<SystemType> userSystemTypes);
        Task<List<LookupDTO>> GetSourceList(string systemType);

        Task<string> GetSubSearchViewAsync(string systemType, string sourceCode);
        Task<string> GetMainViewAsync(string systemType, string sourceCode);

        #endregion
    }
}
