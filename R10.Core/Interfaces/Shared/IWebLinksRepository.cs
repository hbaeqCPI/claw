using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using R10.Core.DTOs;

namespace R10.Core.Interfaces
{
    public interface IWebLinksRepository
    {
        Task<List<WebLinksDTO>> GetWebLinks(int id, string module, string subModule = "FormLink", string subSystem = "");
        Task<WebLinksUrlDTO> GetWebLinksUrl(int mainId, int id, string module, string subModule, string subSystem);
        Task<List<WebLinksNumberTemplateDTO>> GetNumberTemplates(string systemType, string country,
            string caseType, string numberType, string templateType, string templateRank);
        Task<string> GetUrl(string urlCode);
        Task<int> GetMainId(string mainCode, string systemType);
    }
}
