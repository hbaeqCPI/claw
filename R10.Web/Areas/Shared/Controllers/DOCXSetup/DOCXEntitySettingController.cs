//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using AutoMapper;
//using AutoMapper.QueryableExtensions;
//using Kendo.Mvc.Extensions;
//using Kendo.Mvc.UI;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Localization;
//using R10.Core.Entities;
//using R10.Core.Interfaces;
//using R10.Web.Areas.Shared.ViewModels;
//using R10.Web.Extensions;
//using R10.Web.Extensions.ActionResults;
//using R10.Web.Filters;
//using R10.Web.Helpers;
//using R10.Web.Interfaces;
//using R10.Web.Models;
//using R10.Web.Security;

//namespace R10.Web.Areas.Shared.Controllers
//{
//    /* This controller is used in entity screens docx settings */
//    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
//    public class DOCXEntitySettingController : BaseController
//    {
//        private readonly IDOCXEntitySettingRepository _settingRepository;
//        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;
//        private readonly IMapper _mapper;

//        public DOCXEntitySettingController(IDOCXEntitySettingRepository settingRepository, IStringLocalizer<SharedResource> sharedLocalizer, IMapper mapper)
//        {
//            _settingRepository = settingRepository;
//            _mapper = mapper;
//            _sharedLocalizer = sharedLocalizer;
//        }

//        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
//        public async Task<IActionResult> SettingsUpdate([DataSourceRequest] DataSourceRequest request, ContactDOCXesFilterViewModel contactDOCXesFilter, [Bind(Prefix = "updated")]IEnumerable<DOCXEntitySettingViewModel> updatedSettingsVM,
//         [Bind(Prefix = "new")]IEnumerable<DOCXEntitySettingViewModel> newSettingsVM, [Bind(Prefix = "deleted")]IEnumerable<DOCXEntitySettingViewModel> deletedSettingsVM)
//        {
          
//            var deletedSettings = new List<DOCXEntitySetting>();
//            foreach (var item in deletedSettingsVM)
//            {
//                var docxSetting = _mapper.Map<DOCXEntitySetting>(item);
//                deletedSettings.Add(docxSetting);
//            }

//            var updatedSettings = new List<DOCXEntitySetting>();
//            foreach (var item in updatedSettingsVM)
//            {
//                var setting = UpdateViewModel(item);
//                setting.EntityType = contactDOCXesFilter.EntityType;
//                setting.EntityId = contactDOCXesFilter.EntityId;
//                setting.ContactId = contactDOCXesFilter.ContactId;
        
//                updatedSettings.Add(setting);
//            }

//            var newSettings = new List<DOCXEntitySetting>();
//            foreach (var item in newSettingsVM)
//            {
//                var setting = UpdateViewModel(item);
//                setting.EntityType = contactDOCXesFilter.EntityType;
//                setting.EntityId = contactDOCXesFilter.EntityId;
//                setting.ContactId = contactDOCXesFilter.ContactId;

//                newSettings.Add(setting);
//            }
//            if (deletedSettings.Any() || updatedSettings.Any() || newSettings.Any())
//                await _settingRepository.SettingsUpdate(updatedSettings, newSettings, deletedSettings);

//            return Ok();
//        }



//        public async Task<IActionResult> SettingRead([DataSourceRequest] DataSourceRequest request, ContactDOCXesFilterViewModel contactDOCXesFilter)
//        {
//            var vm = await _settingRepository.QueryableList
//                          .Where(s => s.EntityType == contactDOCXesFilter.EntityType && s.EntityId == contactDOCXesFilter.EntityId && s.ContactId == contactDOCXesFilter.ContactId)
//                          .ProjectTo<DOCXEntitySettingViewModel>().ToListAsync();

//            var sendAsOptions = DOCXSendAsOptionViewModel.BuildList(_sharedLocalizer);

//            vm.ForEach(cc =>
//            {
//                cc.DOCXSendAs = cc.SendAs;
//                cc.DOCXSendAsDescription = sendAsOptions.Where(o => o.DOCXSendAs.ToLower() == cc.SendAs.ToLower()).Select(o => o.Description).FirstOrDefault();
//            });
//            return Json(vm.ToDataSourceResult(request));

//        }

//        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
//        public async Task<IActionResult> SettingDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] DOCXEntitySettingViewModel deletedSettingVM)
//        {
//            if (deletedSettingVM.SettingId > 0) {
//                if (!ModelState.IsValid)
//                    return new JsonBadRequest(new { errors = ModelState.Errors() });

//                var docxSetting = _mapper.Map<DOCXEntitySetting>(deletedSettingVM);

//                if (docxSetting.SettingId > 0)
//                    await _settingRepository.DeleteAsync(docxSetting);
//            }
//            return Ok();
//        }


//        protected DOCXEntitySetting UpdateViewModel(DOCXEntitySettingViewModel settingVM)
//        {
//            settingVM.DOCXCatId = settingVM.DOCXCategory.DOCXCatId;
//            settingVM.SendAs = settingVM.DOCXSendAs;
//            var setting = _mapper.Map<DOCXEntitySetting>(settingVM);
//            UpdateEntityStamps(setting, setting.SettingId);
//            return setting;
//        }

//    }

    

//}