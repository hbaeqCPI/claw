using R10.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Extensions.ActionResults
{
    public class NoRecordFoundResult : IActionResult
    {
     
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
            var message = localizer["Your search did not match any records."];

            var response = context.HttpContext.Response;
            response.StatusCode = 400;

            var responseByteArray = Encoding.UTF8.GetBytes(message);
            await response.Body.WriteAsync(responseByteArray, 0, responseByteArray.Length);
            await response.Body.FlushAsync();
        }
    }


}
