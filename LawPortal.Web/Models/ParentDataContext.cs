using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LawPortal.Web.Models
{
    /// <summary>
    /// Resolves parent record data by screen type (Quick Email removed).
    /// </summary>
    public class ParentDataContext
    {
        private readonly ScreenName _screenName;
        private readonly ClaimsPrincipal _user;

        public ParentDataContext(ScreenName screenName, ClaimsPrincipal user)
        {
            _screenName = screenName;
            _user = user;
        }

        public Task<object> GetData(ScreenName screenName, ParentDataStrategyParam param)
        {
            return Task.FromResult<object>(null);
        }
    }
}
