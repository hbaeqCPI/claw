using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.GlobalSearch;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class GlobalSearchViewModelService : IGlobalSearchViewModelService
    {
        private readonly IGlobalSearchService _globalSearchService;
        //private readonly ICPiSystemRoleRepository _cpiSystemRoleRepository;

        public GlobalSearchViewModelService(
                IGlobalSearchService globalSearchService
                //,ICPiSystemRoleRepository cpiSystemRoleRepository
            )
        {
            _globalSearchService = globalSearchService;
            //_cpiSystemRoleRepository = cpiSystemRoleRepository;
        }

        // returns the systems, and screens under each system
        //public async Task<List<GSSystemScreen>> GetSystemScreens(List<SystemType> userSystemTypes)
        //{
        //    List<GSSystemScreen> sysScreens = new List<GSSystemScreen>();
        //    var systems = await GetSystemList(userSystemTypes);
        //    // split into 2 separate operation so the record sorting is preserved; async screen load messes up the insertion of records to sysScreens
        //    systems.Each(s => sysScreens.Add(new GSSystemScreen() { System = s }));
        //    sysScreens.Each(async sc => {
        //        var screens = await GetScreenList(sc.System.Value);
        //        sc.Screens = screens;
        //        Console.WriteLine(sc.Screens.Count);
        //    });
        //    return sysScreens;
        //}

        public async Task<List<GSSystemScreen>> GetSystemScreens(List<SystemType> userSystemTypes)
        {

            // get systems & screens separately, DO NOT NEST, to avoid async & concurrency issues
            List<GSSystemScreen> sysScreens = new List<GSSystemScreen>();
            var systems = await GetSystemList(userSystemTypes);
            var screens = await GetScreenList();

            systems.ForEach(sys => {
                var thisScreens = screens.Where(scr => scr.SystemType == sys.Value).OrderBy(scr => scr.EntryOrder)
                                    .Select(scr => new LookupDTO() { Value = scr.ScreenCode, Text = scr.ScreenName }).ToList();
                //var thisSystem = new LookupDTO() {Value = sys.Value, Text = sys.Text };
                var sysScreen = new GSSystemScreen() { 
                    //System = thisSystem,
                    System = sys,
                    Screens = thisScreens
                };
                sysScreens.Add(sysScreen);
            });

            return sysScreens;
        }

        public async Task<List<GSDataCriteriaViewModel>> GetFieldDataSource(List<SystemType> userSystemTypes, bool isLoadAll)
        {
            var list = _globalSearchService.GSFields
                            .Where(f => f.IsEnabled && (f.IsAutoSearch || isLoadAll)
                                && f.GSScreen.IsEnabled && f.GSScreen.GSSystem.IsEnabled
                                && !f.GSTable.IsDocContent
                             )
                            .Select(d => new
                            {
                                d.GSScreen.GSSystem.SystemType,
                                d.FieldId,
                                d.FieldLabelLong,
                                GSSystemEntryOrder = d.GSScreen.GSSystem.EntryOrder,
                                GSScreenEntryOrder = d.GSScreen.EntryOrder,
                                d.EntryOrder
                            })
                            .AsEnumerable()
                            .Where(d => userSystemTypes.Any(us => us.TypeId == d.SystemType))
                            .OrderBy(f => f.GSSystemEntryOrder).ThenBy(f => f.GSScreenEntryOrder).ThenBy(f => f.EntryOrder)
                            .Select(f => new GSDataCriteriaViewModel
                            {
                                LogicalOperator = "OR",
                                FieldId = f.FieldId,
                                Criteria = "",
                                Field = new GSFieldListViewModel() { FieldId = f.FieldId, FieldLabel = f.FieldLabelLong }
                            }).ToList();
            if (list.Count > 0)
            {
                list[0].LogicalOperator = "";
            }
            return list;
        }

        public async Task<List<GSFieldListViewModel>> GetFieldList(List<SystemType> userSystemTypes, bool isDocContent = false, bool defaultCriteriaOnly = false)
        {            
            var list = _globalSearchService.GSFields
                                .Where(f => f.IsEnabled && f.GSScreen.IsEnabled 
                                        && f.GSScreen.GSSystem.IsEnabled 
                                        && (f.GSTable.IsDocContent == isDocContent) 
                                        && (!defaultCriteriaOnly || f.IsDefaultCriteria == defaultCriteriaOnly))
                                .OrderBy(f => f.GSScreen.GSSystem.EntryOrder).ThenBy(f => f.GSScreen.EntryOrder).ThenBy(f => f.EntryOrder)                
                                .Select(d => new
                                {
                                    d.GSScreen.GSSystem.SystemType,
                                    d.FieldId,
                                    d.FieldLabelLong
                                })
                                .AsEnumerable()
                                .Where(d => userSystemTypes.Any(us => us.TypeId == d.SystemType))
                                .Select(f => new GSFieldListViewModel() {
                                    FieldId = f.FieldId, 
                                    FieldLabel = f.FieldLabelLong 
                                })
                                .ToList();
            return list;
        }

        public async Task<List<GSDocCriteriaViewModel>> GetDocDataSource(List<SystemType> userSystemTypes, bool isLoadAll)
        {
            var list = _globalSearchService.GSFields
                            .Where(f => f.IsEnabled && (f.IsAutoSearch || isLoadAll)
                                && f.GSScreen.IsEnabled && f.GSScreen.GSSystem.IsEnabled
                                && f.GSTable.IsDocContent
                            )
                            .Select(d => new
                            {
                                d.GSScreen.GSSystem.SystemType,
                                d.FieldId,
                                d.FieldLabelLong,
                                GSSystemEntryOrder = d.GSScreen.GSSystem.EntryOrder,
                                GSScreenEntryOrder = d.GSScreen.EntryOrder,
                                d.EntryOrder
                            })
                            .AsEnumerable()
                            .Where(d => userSystemTypes.Any(us => us.TypeId == d.SystemType))
                            .OrderBy(f => f.GSSystemEntryOrder).ThenBy(f => f.GSScreenEntryOrder).ThenBy(f => f.EntryOrder)
                            .Select(f => new GSDocCriteriaViewModel
                            {
                                FieldId = f.FieldId,
                                Criteria = "",
                                Field = new GSFieldListViewModel() { FieldId = f.FieldId, FieldLabel = f.FieldLabelLong }
                            }).ToList();

            return list;
        }

        private async Task<List<LookupDTO>> GetSystemList(List<SystemType> userSystemTypes)
        {
            //var list = await _globalSearchService.GSSystems.Where(gs => gs.IsEnabled).OrderBy(s => s.EntryOrder)
            //                .Select(gs => new LookupDTO() { Value = gs.SystemType, Text = gs.SystemName }).ToListAsync();
            //var list = await _globalSearchService.GSSystems.Where(gs => gs.IsEnabled && userSystemTypes.Any(us => us.TypeId == gs.SystemType)).OrderBy(s => s.EntryOrder)
            //               .Select(gs => new LookupDTO() { Value = gs.SystemType, Text = gs.SystemName }).ToListAsync();
            var list = _globalSearchService.GSSystems.Where(gs => gs.IsEnabled)
                                .OrderBy(s => s.EntryOrder)
                                .Select(d => new
                                {
                                    d.SystemType,
                                    d.SystemName
                                })
                                .AsEnumerable()
                                .Where(d => userSystemTypes.Any(us => us.TypeId == d.SystemType))
                                .Select(gs => new LookupDTO() { Value = gs.SystemType, Text = gs.SystemName })
                                .ToList();
            return list;
        }

        private async Task<List<GSScreen>> GetScreenList()
        {
            var list =  await _globalSearchService.GSScreens.Where(s => s.IsEnabled).ToListAsync();
            return list;
        }

        //private async Task<List<LookupDTO>> GetSystemList(List<SystemType> userSystemTypes)
        //{
        //    var list = await _globalSearchService.GSSystems.Where(gs => gs.IsEnabled && userSystemTypes.Any(us => us.TypeId == gs.SystemType)).OrderBy(s => s.EntryOrder)
        //                    .Select(gs => new LookupDTO() { Value = gs.SystemType, Text = gs.SystemName }).ToListAsync();

        //    return list;
        //}

        //private async Task<List<LookupDTO>> GetScreenList(string systemType)
        //{
        //    var list = await _globalSearchService.GSScreens.Where(s => s.SystemType == systemType && s.IsEnabled).OrderBy(s => s.EntryOrder)
        //                    .Select(s => new LookupDTO() { Value = s.ScreenCode, Text = s.ScreenName }).ToListAsync();
        //    return list;
        //}


    }
}
