using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Extensions;
using R10.Web.Models.DeleteConfirmationViewModels;

namespace R10.Web.Controllers
{
    [Authorize]
    public class DeleteConfirmationController : Microsoft.AspNetCore.Mvc.Controller
    {
        public IActionResult Index()
        {
            if (!Request.IsAjax())
                return new BadRequestResult();

            ViewData.Model = new CheckboxViewModel();
            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_Checkbox",
                ViewData = ViewData,
                TempData = TempData
            };
        }
        public IActionResult ConfirmationCode()
        {
            if (!Request.IsAjax())
                return new BadRequestResult();

            ViewData.Model = new ConfirmationCodeViewModel()
            {
                ConfirmationToken = GenerateToken(6)
            };
            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_ConfirmationCode",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        public string GenerateToken(int length)
        {
            const string chars = "0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}