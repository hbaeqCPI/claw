using System.Text.RegularExpressions;
using R10.Core.DTOs;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;

namespace R10.Core.Services.Shared
{
    public class WebLinksService : IWebLinksService
    {
        private const string StartTag = "<<";
        private const string EndTag = ">>";

        private readonly IWebLinksRepository _repository;
        private readonly INumberFormatService _numberFormatService;
        public WebLinksService(IWebLinksRepository repository, INumberFormatService numberFormatService)
        {
            _repository = repository;
            _numberFormatService = numberFormatService;
        }

        public async Task<List<WebLinksDTO>> GetWebLinks(int id, string module, string subModule, string subSystem)
        {
            return await _repository.GetWebLinks(id, module, subModule, subSystem);
        }

        public async Task<string> GetWebLinksUrl(int mainId, int id, string module, string subModule, string subSystem)
        {
            var searchInfo = await _repository.GetWebLinksUrl(mainId, id, module, subModule, subSystem);

            if (searchInfo == null)
                return null;

            if (!string.IsNullOrEmpty(searchInfo.UrlExpr))
            {
                var numberInfo = BuildNumberInfo(searchInfo);
                searchInfo.ActiveUrl = searchInfo.UrlExpr;

                var tags = new List<string>();
                foreach (Match match in Regex.Matches(searchInfo.UrlExpr, $"{StartTag}(.*?){EndTag}"))
                {
                    tags.Add(match.Groups[1].Value);
                }

                foreach (var tag in tags)
                {
                    var rank = "0001";
                    var numberTag = tag;

                    if (tag.IndexOf(":", StringComparison.Ordinal) > -1)
                    {
                        rank = tag.Substring(tag.IndexOf(":", StringComparison.Ordinal) + 1);
                        numberTag = tag.Substring(0, tag.IndexOf(":", StringComparison.Ordinal));
                    }

                    numberTag = numberTag.ToLower();
                    switch (numberTag)
                    {
                        case WebLinksNumberTag.AppNo:
                            numberInfo.Number = _numberFormatService.CleanUpNumber(numberInfo.AppNumber);
                            numberInfo.NumberType = WebLinksNumberType.AppNo;
                            numberInfo.NumberDate = numberInfo.FilDate;
                            break;

                        case WebLinksNumberTag.PubNo:
                            numberInfo.Number = _numberFormatService.CleanUpNumber(numberInfo.PubNumber);
                            numberInfo.NumberType = WebLinksNumberType.PubNo;
                            numberInfo.NumberDate = numberInfo.PubDate;
                            break;

                        case WebLinksNumberTag.PatRegNo:
                            numberInfo.Number = _numberFormatService.CleanUpNumber(numberInfo.PatRegNumber);
                            numberInfo.NumberType = WebLinksNumberType.PatRegNo;
                            numberInfo.NumberDate = numberInfo.IssRegDate;
                            break;
                    }

                    if (string.IsNullOrEmpty(numberInfo.Number))
                        return null;

                    numberInfo.SystemType = WebLinksSystemType.Patent;
                    var trademarkModules = new string[] { "trademark", "tmkcountrylaw" };
                    if (trademarkModules.Contains(module.ToLower()))
                    {
                        numberInfo.SystemType = WebLinksSystemType.Trademark;
                    }
                    var formattedNumber = await _numberFormatService.FormatNumber(numberInfo, WebLinksTemplateType.Web, rank);
                    searchInfo.ActiveUrl = searchInfo.ActiveUrl.Replace($"{StartTag}{tag}{EndTag}",formattedNumber);

                    //maybe tags are same number
                    if (tags.Count > 1)
                    {
                        var matches = Regex.Matches(searchInfo.ActiveUrl, $"{StartTag}(.*?){EndTag}");
                        if (matches.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
            return searchInfo.ActiveUrl;
        }

        public async Task<string> GetUrl(string urlCode) {
            return await _repository.GetUrl(urlCode);
        }

        public async Task<int> GetMainId(string mainCode, string systemType)
        {
            return await _repository.GetMainId(mainCode,systemType);
        }

        private WebLinksNumberInfoDTO BuildNumberInfo(WebLinksUrlDTO searchInfo)
        {
            return new WebLinksNumberInfoDTO
            {
                Country = searchInfo.Country,
                CaseType = searchInfo.CaseType,
                AppNumber = searchInfo.AppNumber,
                PubNumber = searchInfo.PubNumber,
                PatRegNumber = searchInfo.PatRegNumber,
                FilDate = searchInfo.FilDate,
                PubDate = searchInfo.PubDate,
                IssRegDate = searchInfo.IssRegDate,
            };
        }

    }

    public static class WebLinksNumberTag
    {
        public const string AppNo = "appno";
        public const string PubNo = "pubno";
        public const string PatRegNo = "patregno";
    }


}
