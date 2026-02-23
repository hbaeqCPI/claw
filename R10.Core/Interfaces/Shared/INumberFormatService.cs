using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Core.Interfaces.Shared
{
    public interface INumberFormatService
    {
        Task<WebLinksParsedInfoDTO> StandardizeNumber(WebLinksNumberInfoDTO numberInfo);
        Task<List<WebLinksNumberTemplateDTO>> GetNumberTemplates(string systemType, string country,
           string caseType, string numberType, string templateType, string templateRank);
        WebLinksParsedInfoDTO ParseNumber(List<WebLinksNumberTemplateDTO> standardTemplates, WebLinksNumberInfoDTO numberInfo, int digitCount = 0);

        string FormatNumber(WebLinksNumberInfoDTO numberInfo, List<WebLinksNumberTemplateDTO> standardTemplates,
            string targetTemplate);

        Task<string> FormatNumber(WebLinksNumberInfoDTO numberInfo, string templateType, string templateRank = "");

        string CleanUpNumber(string userNumber);

    }
}
