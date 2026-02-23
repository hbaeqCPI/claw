using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class FormExtractViewModelService : IFormExtractViewModelService
    {
        private readonly IFormExtractService _formExtractService;


        public FormExtractViewModelService(IFormExtractService formExtractService)
        {
            _formExtractService = formExtractService;
        }


        #region Search
        public async Task<List<LookupDTO>> GetSystemList(List<SystemType> userSystemTypes)
        {
            var list = await _formExtractService.FormSystems.Where(s => s.IsEnabled).OrderBy(s => s.EntryOrder).ToListAsync();
            var lookup = list.Where(s => userSystemTypes.Any(us => us.TypeId == s.SystemType))
                        .Select(s => new LookupDTO() { Value = s.SystemType, Text = s.SystemName }).ToList();
            return lookup;
        }

        public async Task<List<LookupDTO>> GetSourceList(string systemType)
        {
            var list = await _formExtractService.FRSources.Where(s => s.SystemType == systemType && s.IsEnabled).OrderBy(t => t.EntryOrder)
                            .Select(s => new LookupDTO() { Value = s.SourceCode, Text = s.SourceName }).ToListAsync();
            return list;
        }

        public async Task<string> GetSubSearchViewAsync(string systemType, string sourceCode)
        {
            var view = await _formExtractService.FRSources.Where(s => s.SystemType == systemType && s.SourceCode == sourceCode).Select(t => t.SearchTabView).FirstOrDefaultAsync();
            return view;
        }

        public async Task<string> GetMainViewAsync(string systemType, string sourceCode)
        {
            var view = await _formExtractService.FRSources.Where(s => s.SystemType == systemType && s.SourceCode == sourceCode).Select(t => t.MainView).FirstOrDefaultAsync();
            return view;
        }

        #endregion



    }
}
