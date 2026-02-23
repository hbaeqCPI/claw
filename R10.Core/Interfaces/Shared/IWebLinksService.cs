using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Core.Interfaces.Shared
{
    public interface IWebLinksService
    {
        Task<List<WebLinksDTO>> GetWebLinks(int id, string module, string subModule, string subSystem);
        Task<string> GetWebLinksUrl(int mainId, int id, string module, string subModule, string subSystem);
        Task<string> GetUrl(string urlCode);
        Task<int> GetMainId(string mainCode, string systemType);
    }
}
