using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Security;
using R10.Web.Services;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class MapController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IMapService _service;

        public MapController(IMapService service)
        {
            _service = service;
        }

        [HttpGet()]
        public async Task<IActionResult> Invention(int id)
        {
            return  await Index(nameof(Invention),id);
        }

        public async Task<IActionResult> Application(int id)
        {
            return await Index(nameof(Application), id);
        }

        public async Task<IActionResult> Trademark(int id)
        {
            return await Index(nameof(Trademark), id);
        }

        [HttpGet]
        private async Task<IActionResult> Index(string screen,int id)
        {
            ViewBag.Screen = screen;
            ViewBag.Id = id;
            var entries = await _service.GetMarkerById(screen, id);
            if (Request.IsAjax())
                return PartialView("Index", entries);
            else
                return View("Index",entries);
            
            
        }

    }
}