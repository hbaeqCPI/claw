using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
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
    /* This controller is used in entity screens letter settings */
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class LetterEntitySettingController : BaseController
    {
        private readonly ILetterEntitySettingRepository _settingRepository;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
        private readonly IMapper _mapper;

        public LetterEntitySettingController(ILetterEntitySettingRepository settingRepository, IStringLocalizer<SharedResource> sharedLocalizer, IMapper mapper)
        {
            _settingRepository = settingRepository;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> SettingsUpdate([DataSourceRequest] DataSourceRequest request, ContactLettersFilterViewModel contactLettersFilter, [Bind(Prefix = "updated")]IEnumerable<LetterEntitySettingViewModel> updatedSettingsVM,
         [Bind(Prefix = "new")]IEnumerable<LetterEntitySettingViewModel> newSettingsVM, [Bind(Prefix = "deleted")]IEnumerable<LetterEntitySettingViewModel> deletedSettingsVM)
        {
          
            var deletedSettings = new List<LetterEntitySetting>();
            foreach (var item in deletedSettingsVM)
            {
                var letterSetting = _mapper.Map<LetterEntitySetting>(item);
                deletedSettings.Add(letterSetting);
            }

            var updatedSettings = new List<LetterEntitySetting>();
            foreach (var item in updatedSettingsVM)
            {
                var setting = UpdateViewModel(item);
                setting.EntityType = contactLettersFilter.EntityType;
                setting.EntityId = contactLettersFilter.EntityId;
                setting.ContactId = contactLettersFilter.ContactId;
        
                updatedSettings.Add(setting);
            }

            var newSettings = new List<LetterEntitySetting>();
            foreach (var item in newSettingsVM)
            {
                var setting = UpdateViewModel(item);
                setting.EntityType = contactLettersFilter.EntityType;
                setting.EntityId = contactLettersFilter.EntityId;
                setting.ContactId = contactLettersFilter.ContactId;

                newSettings.Add(setting);
            }
            if (deletedSettings.Any() || updatedSettings.Any() || newSettings.Any())
                await _settingRepository.SettingsUpdate(updatedSettings, newSettings, deletedSettings);

            return Ok();
        }



        public async Task<IActionResult> SettingRead([DataSourceRequest] DataSourceRequest request, ContactLettersFilterViewModel contactLettersFilter)
        {
            var vm = await _settingRepository.QueryableList
                          .Where(s => s.EntityType == contactLettersFilter.EntityType && s.EntityId == contactLettersFilter.EntityId && s.ContactId == contactLettersFilter.ContactId)
                          .ProjectTo<LetterEntitySettingViewModel>().ToListAsync();

            var sendAsOptions = SendAsOptionViewModel.BuildList(_sharedLocalizer);

            vm.ForEach(cc =>
            {
                cc.LetterSendAs = cc.SendAs;
                cc.LetterSendAsDescription = sendAsOptions.Where(o => o.LetterSendAs.ToLower() == cc.SendAs.ToLower()).Select(o => o.Description).FirstOrDefault();
            });
            return Json(vm.ToDataSourceResult(request));

        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> SettingDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] LetterEntitySettingViewModel deletedSettingVM)
        {
            if (deletedSettingVM.SettingId > 0) {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                var letterSetting = _mapper.Map<LetterEntitySetting>(deletedSettingVM);

                if (letterSetting.SettingId > 0)
                    await _settingRepository.DeleteAsync(letterSetting);
            }
            return Ok();
        }


        protected LetterEntitySetting UpdateViewModel(LetterEntitySettingViewModel settingVM)
        {
            settingVM.LetCatId = settingVM.LetterCategory.LetCatId;
            settingVM.SendAs = settingVM.LetterSendAs;
            var setting = _mapper.Map<LetterEntitySetting>(settingVM);
            UpdateEntityStamps(setting, setting.SettingId);
            return setting;
        }

    }

    

}