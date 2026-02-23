using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Filters;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{

    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ClientContactController : BaseController
    {
        private readonly IClientViewModelService _clientViewModelService;
        private readonly IClientService _clientService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ClientContactController(IClientViewModelService clientViewModelService, IClientService clientService,
            IMapper mapper,
            IStringLocalizer<SharedResource> localizer)
        {
            _clientViewModelService = clientViewModelService;
            _clientService = clientService;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<IActionResult> ContactRead([DataSourceRequest] DataSourceRequest request, int clientId)
        {
            var result = (await _clientViewModelService.GetClientContacts(clientId)).ToDataSourceResult(request);
            return Json(result);
        }


        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> ContactsUpdate(int clientId, 
            [Bind(Prefix = "updated")]IEnumerable<ClientContactViewModel> updated,
            [Bind(Prefix = "new")]IEnumerable<ClientContactViewModel> added, 
            [Bind(Prefix = "deleted")]IEnumerable<ClientContactViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _clientService.ChildService.Update(clientId, User.GetUserName(),
                    _mapper.Map<List<ClientContact>>(updated),
                    _mapper.Map<List<ClientContact>>(added),
                    _mapper.Map<List<ClientContact>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Contact has been saved successfully."].ToString() :
                    _localizer["Contacts have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> ContactDelete([Bind(Prefix = "deleted")] ClientContactViewModel deleted)
        {
            if (deleted.ClientContactID >= 0)
            {
                await _clientService.ChildService.Update(deleted.ClientID, User.GetUserName(), new List<ClientContact>(), new List<ClientContact>(), new List<ClientContact>() { _mapper.Map<ClientContact>(deleted) });
                return Ok(new { success = _localizer["Contact has been deleted successfully."].ToString() });
            }
            return Ok();
        }
    }
}