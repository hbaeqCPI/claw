using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using R10.Core.DTOs;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace R10.Infrastructure.Data
{

    public class WebLinksRepository:IWebLinksRepository 
    {
        private readonly ApplicationDbContext _dbContext;
        public WebLinksRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<WebLinksDTO>> GetWebLinks(int id, string module, string subModule = "FormLink", string subSystem = "")
        {
            var action = GetAction(module);

            if (action == 0)
                return null;

            var webLinks = await _dbContext.WebLinksDTO.FromSqlInterpolated($"procSysWebLinks @Action={action}, @SubModule={subModule}, @KeyID={id},@SubSystem={subSystem}").AsNoTracking().ToListAsync();
            return webLinks;

        }

        public async Task<WebLinksUrlDTO> GetWebLinksUrl(int mainId, int id, string module, string subModule, string subSystem)
        {
            var action = GetAction(module);
            if (action == 0)
                return null;

            var webLinksUrl = await _dbContext.WebLinksUrlDTO.FromSqlInterpolated($"procSysWebLinksUrl @Action={action}, @MainId={mainId}, @KeyID={id},@SubSystem={subSystem}").AsNoTracking().ToListAsync();
            return webLinksUrl.Count > 0 ? webLinksUrl[0] : null;
        }

        public async Task<List<WebLinksNumberTemplateDTO>> GetNumberTemplates(string systemType, string country,
            string caseType, string numberType, string templateType, string templateRank)
        {
            var templates = await _dbContext.WebLinksNumberTemplateDTO.FromSqlInterpolated($"procSysWebLinksGetNumberTemplate @SystemType={systemType},@Country={country}, @CaseType={caseType},@NumberType={numberType},@TemplateType={templateType}, @TemplateRank={templateRank}").AsNoTracking().ToListAsync();
            return templates;
        }


        public async Task<string> GetUrl(string urlCode)
        {
            var result = await _dbContext.LookupDTO.FromSqlInterpolated($"Select UrlExpr as Value,'' as Text From tblWLExpr Where UrlCode={urlCode}").FirstOrDefaultAsync();
            return result?.Value ?? "";
        }

        public async Task<int> GetMainId(string mainCode,string systemType) {
            var result = await _dbContext.LookupDTO.FromSqlInterpolated($"Select Cast(MainId as varchar) as Value,'' as Text From tblWLMain Where MainCode={mainCode} and SystemType={systemType}").FirstOrDefaultAsync();
            if (result !=null)
                return Convert.ToInt32(result?.Value);
            return 0;
        }

        private int GetAction(string module)
        {
            module = module.ToLower();
            switch (module)
            {
                case "patent":
                    return  100;

                case "trademark":
                    return 200;

                case "generalmatter":
                    return 300;

                case "ams":
                    return 110;

                case "patcountrylaw":
                    return 150;

                case "tmkcountrylaw":
                    return 250;

                default:
                    return 0;
            }

        }
    }
}


