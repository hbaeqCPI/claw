using System;
using System.Security.Claims;
using System.Threading.Tasks;
using R10.Web.Interfaces;
using R10.Web.Models;

namespace R10.Web.Models
{
    /// <summary>
    /// Resolves parent record data for Quick Email / DocuSign by screen type.
    /// </summary>
    public class ParentDataContext
    {
        private readonly IOuickEmailViewModelService _quickEmailViewModelService;
        private readonly ScreenName _screenName;
        private readonly ClaimsPrincipal _user;

        public ParentDataContext(IOuickEmailViewModelService quickEmailViewModelService, ScreenName screenName, ClaimsPrincipal user)
        {
            _quickEmailViewModelService = quickEmailViewModelService;
            _screenName = screenName;
            _user = user;
        }

        public async Task<object> GetData(ScreenName screenName, ParentDataStrategyParam param)
        {
            if (param == null) return null;
            try
            {
                switch (screenName)
                {
                    case ScreenName.PatInvention: return await _quickEmailViewModelService.GetPatInvention(param);
                    case ScreenName.PatCountryApplication: return await _quickEmailViewModelService.GetPatCountryApplication(param);
                    case ScreenName.TmkTrademark: return await _quickEmailViewModelService.GetTmkTrademark(param);
                    case ScreenName.GmMatter: return await _quickEmailViewModelService.GetGmMatter(param);
                    case ScreenName.PatActionDue:
                    case ScreenName.PatActionDueInv:
                    case ScreenName.TmkActionDue:
                    case ScreenName.GmActionDue:
                    case ScreenName.PatCostTracking:
                    case ScreenName.PatCostTrackingInv:
                    case ScreenName.TmkCostTracking:
                    case ScreenName.GmCostTracking:
                    case ScreenName.PatActionDueDate:
                    case ScreenName.PatActionDueDateInv:
                    case ScreenName.TmkActionDueDate:
                    case ScreenName.GmActionDueDate:
                    default: return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
