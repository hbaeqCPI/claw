using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
// using R10.Core.Interfaces.DMS; // Removed during deep clean
using R10.Core.Interfaces.Shared;
using R10.Web.Areas.Shared.Controllers;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Filters;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.SharePoint;
using System.Globalization;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;

namespace R10.Web.Api.Shared
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [ApiController]
    public class CurrencyTypesController : ControllerBase
    {
        private readonly ServiceAccount _serviceAccount;
        private readonly ICPIGoogleService _cpiGoogleService;
        private readonly ILogger<CurrencyTypesController> _logger;

        public CurrencyTypesController(
            IOptions<ServiceAccount> serviceAccount,
            ICPIGoogleService cpiGoogleService,
            ILogger<CurrencyTypesController> logger
            )
        {
            _serviceAccount = serviceAccount.Value;
            _cpiGoogleService = cpiGoogleService;
            _logger = logger;
        }

        [HttpPost("UpdateExchangeRates")]        
        public async Task<IActionResult> UpdateExchangeRates()
        {
            //Only allow scheduler or cpiAdmin users
            var userEmail = User.GetEmail();
            if (userEmail != _serviceAccount.UserName && !(User.IsSuper())) return Unauthorized();

            try
            {
                var resultCount = await _cpiGoogleService.UpdateCurrencyExRates();
                return Ok(resultCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
