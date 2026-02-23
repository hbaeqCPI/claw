using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Api.Models;
using R10.Web.Filters;
using System.Text.Json;

namespace R10.Web.Api
{
    [ServiceFilter(typeof(ExceptionFilter))]
    public class WebApiControllerBase : ControllerBase
    {
        protected readonly IEntityService<WebServiceLog> _logService;

        public WebApiControllerBase(IEntityService<WebServiceLog> logService)
        {
            _logService = logService;
        }

        protected async Task<int> InitWebApiLog()
        {
            WebServiceLog log = new WebServiceLog()
            {
                RequestPath = Request.GetEncodedPathAndQuery(),
                LoginName = User.GetUserName(),
                RunDate = DateTime.Now, //execution start timestamp
                Status = CPiWebApiStatus.NotSet.ToString(),
                Method = Request.Method
            };

            await _logService.Add(log);

            return log.LogId;
        }

        protected async Task UpdateWebApiLog(int logId, CPiWebApiStatus statusType, int recCount = 0)
        {
            await UpdateWebApiLog(logId, statusType, recCount, "");
        }

        protected async Task UpdateWebApiLog(int logId, CPiWebApiStatus statusType, int recCount, string remarks)
        {
            var log = await _logService.GetByIdAsync(logId);
            if (log != null)
            {
                log.Status = statusType.ToString();
                log.RecordCount = recCount;
                log.EndDate = DateTime.Now; //execution end timestamp
                log.Remarks = remarks;
                await _logService.Update(log);
            }
        }

        protected ApiError GetWebApiError(WebApiValidationException exception)
        {
            return new ApiError(exception.Message, JsonSerializer.Deserialize<List<string>>(exception.InnerException?.Message ?? ""));
        }
    }

    public enum CPiWebApiStatus
    {
        NotSet,
        Success,
        Failed,
        NotFound
    }
}
