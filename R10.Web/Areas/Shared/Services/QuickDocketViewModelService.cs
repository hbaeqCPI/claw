using R10.Web.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Queries.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ActiveQueryBuilder.Web.Server.Infrastructure;
using R10.Core.Entities.ReportScheduler;

namespace R10.Web.Services
{
    public class QuickDocketViewModelService : IOuickDocketViewModelService
    {
        
        private readonly IQuickDocketRepository _quickDocketRepository;
        private readonly IMapper _mapper;

        public QuickDocketViewModelService(
                                IQuickDocketRepository quickDocketRepository,
                                IMapper mapper)                                
        {
            _quickDocketRepository = quickDocketRepository;
            _mapper = mapper;            
        }
                      
        public QuickDocketSearchCriteriaViewModel GetSearchCriteria(QuickDocketDefaultSettingsViewModel defaultSettings)
        {
            var searchCriteria = _mapper.Map<QuickDocketSearchCriteriaViewModel>(defaultSettings);

            ComputeDates(defaultSettings, ref searchCriteria);
            return searchCriteria;
        }

        public async Task<List<QuickDocketDTO>> GetQuickDocket(QuickDocketSearchCriteriaViewModel viewModel)
        {
            var criteria = _mapper.Map<QuickDocketSearchCriteriaDTO>(viewModel);
            criteria.SystemTypes = GetSystemTypes(viewModel);
            var list = await _quickDocketRepository.GetQuickDocket(criteria);
            return list;
        }

        public async Task UpdateQuickDocket(QuickDocketSearchCriteriaViewModel viewModel, string dateType, DateTime? specificDate,string updatedBy, List<string> recIds)
        {
            var searchCriteria = _mapper.Map<QuickDocketSearchCriteriaDTO>(viewModel);
            var updateCriteria = _mapper.Map<QuickDocketUpdateCriteriaDTO>(searchCriteria);
            updateCriteria.DateType = dateType;
            updateCriteria.SpecificDate = specificDate;
            updateCriteria.UpdatedBy = updatedBy;
            updateCriteria.RecIds = recIds;

            updateCriteria.SystemTypes = GetSystemTypes(viewModel);
            await _quickDocketRepository.UpdateQuickDocket(updateCriteria);
        }

        public async Task<List<QuickDocketDeDocketBatchUpdateResultDTO>> UpdateQuickDocketDedocketInstruction(QuickDocketSearchCriteriaViewModel viewModel, string instruction, string? remarks, bool emptyInstructionOnly, string updatedBy, string userId, List<string> recIds)
        {
            var searchCriteria = _mapper.Map<QuickDocketSearchCriteriaDTO>(viewModel);
            var updateCriteria = _mapper.Map<QuickDocketUpdateCriteriaDTO>(searchCriteria);
            updateCriteria.NewDeDocketInstruction = instruction;
            updateCriteria.NewDeDocketRemarks = remarks;
            updateCriteria.EmptyInstructionOnly = emptyInstructionOnly;
            updateCriteria.UpdatedBy = updatedBy;
            updateCriteria.UserId = userId;
            updateCriteria.RecIds = recIds;

            updateCriteria.SystemTypes = GetSystemTypes(viewModel);
            return await _quickDocketRepository.UpdateQuickDocketDeDocketBatch(updateCriteria);
        }

        public List<QuickDocketSchedulerViewModel> GetQuickDocketScheduler(QuickDocketSearchCriteriaViewModel viewModel)
        {
            var criteria = _mapper.Map<QuickDocketSearchCriteriaDTO>(viewModel);
            criteria.SystemTypes = GetSystemTypes(viewModel);
            criteria.TargetData = "cal";

            if (viewModel.AttorneyFilter1 == null && viewModel.AttorneyFilter2 == null && viewModel.AttorneyFilter3 == null && viewModel.AttorneyFilter4 == null && viewModel.AttorneyFilter5 == null && viewModel.AttorneyFilterR == null && viewModel.AttorneyFilterD == null)
            {
                criteria.FilterAtty = $"|{viewModel.FilterAtty}|";
            }
            return _quickDocketRepository.GetQuickDocketScheduler(criteria).ProjectTo<QuickDocketSchedulerViewModel>().ToList();
        }

        private QuickDocketSearchCriteriaDTO ConvertFilterToSearchCriteria(List<QueryFilterViewModel> filters)
        {
            QuickDocketSearchCriteriaDTO criteria = new QuickDocketSearchCriteriaDTO();
            Type type = criteria.GetType();
            PropertyInfo[] properties = type.GetProperties();

            // handle CountryOp &IndicatorOp
            foreach (PropertyInfo property in properties)
            {
                foreach (var filter in filters)
                {
                    if (property.Name == filter.Property)
                    {
                        if (property.PropertyType == typeof(DateTime?))
                            property.SetValue(criteria, Convert.ToDateTime(filter.Value));
                        else if (property.PropertyType == typeof(Int32?))
                            property.SetValue(criteria, Convert.ToInt32(filter.Value));
                        else
                            property.SetValue(criteria, filter.Value);
                    }
                }
            }

            return criteria;
        }
        
        public async Task<List<QDActionTypeLookupDTO>> GetCombinedActionTypes(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedActionTypes(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(a => a.ActionType.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDActionDueLookupDTO>> GetCombinedActionDues(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedActionDues(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(a => a.ActionDue.StartsWith(text)).ToList();
            return result;
        }
        public async Task<List<QDCaseNumberLookupDTO>> GetCombinedCaseNumbers(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedCaseNumbers(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.CaseNumber.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDCaseTypeLookupDTO>> GetCombinedCaseTypes(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedCaseTypes(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.CaseType.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDRespOfficeLookupDTO>> GetCombinedRespOffices(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedRespOffices(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.RespOffice.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDClientRefLookupDTO>> GetCombinedClientRefs(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedClientRefs(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.ClientRef.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDDeDocketInstructionLookupDTO>> GetCombinedDeDocketInstructions(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDeDocketInstructions(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.Instruction.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDDeDocketInstructedByLookupDTO>> GetCombinedDeDocketInstructedBy(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDeDocketInstructedBy(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.InstructedBy.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDStatusLookupDTO>> GetCombinedStatuses(string systemType,string text)
        {
            var result = await _quickDocketRepository.CombinedStatuses(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(s => s.Status.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDTitleLookupDTO>> GetCombinedTitles(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedTitles(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(t => t.Title.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDIndicatorLookupDTO>> GetCombinedIndicators(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedIndicators(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(i => i.Indicator.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDCountryLookupDTO>> GetCombinedCountries(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedCountries(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.Country.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDActionTypeLookupDTO>> GetCombinedDefaultActionTypes(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultActionTypes(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(a => a.ActionType.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDActionDueLookupDTO>> GetCombinedDefaultActionDues(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultActionDues(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(a => a.ActionDue.StartsWith(text)).ToList();
            return result;
        }
        public async Task<List<QDCaseNumberLookupDTO>> GetCombinedDefaultCaseNumbers(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultCaseNumbers(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.CaseNumber.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDCaseTypeLookupDTO>> GetCombinedDefaultCaseTypes(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultCaseTypes(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.CaseType.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDRespOfficeLookupDTO>> GetCombinedDefaultRespOffices(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultRespOffices(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.RespOffice.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDClientRefLookupDTO>> GetCombinedDefaultClientRefs(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultClientRefs(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.ClientRef.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDStatusLookupDTO>> GetCombinedDefaultStatuses(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultStatuses(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(s => s.Status.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDTitleLookupDTO>> GetCombinedDefaultTitles(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultTitles(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(t => t.Title.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDIndicatorLookupDTO>> GetCombinedDefaultIndicators(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultIndicators(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(i => i.Indicator.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDCountryLookupDTO>> GetCombinedDefaultCountries(string systemType, string text)
        {
            var result = await _quickDocketRepository.CombinedDefaultCountries(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.Country.StartsWith(text)).ToList();
            return result;
        }

        public async Task<List<QDClientLookupDTO>> GetClientList(string systemType, string text) {
            var result = await _quickDocketRepository.GetClientList(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.Client.StartsWith(text)).ToList();
            return result;
        }
        public async Task<List<QDAgentLookupDTO>> GetAgentList(string systemType, string text) {
            var result = await _quickDocketRepository.GetAgentList(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.Agent.StartsWith(text)).ToList();
            return result;
        }
        public async Task<List<QDOwnerLookupDTO>> GetOwnerList(string systemType, string text) {
            var result = await _quickDocketRepository.GetOwnerList(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.Owner.StartsWith(text)).ToList();
            return result;
        }
        public async Task<List<QDAttorneyLookupDTO>> GetAttorneyList(string systemType, string text) {
            var result = await _quickDocketRepository.GetAttorneyList(systemType);
            if (!string.IsNullOrEmpty(text)) result = result.Where(c => c.Attorney.StartsWith(text)).ToList();
            return result;
        }


        private string GetSystemTypes(QuickDocketSearchCriteriaViewModel criteria)
        {
            var systemTypes = "|";
                if (criteria.Patent == "P")
                    systemTypes = systemTypes + criteria.Patent + "|";

                if (criteria.PTOActions == "L")
                    systemTypes = systemTypes + criteria.PTOActions + "|";

                if (criteria.Trademark == "T")
                    systemTypes = systemTypes + criteria.Trademark + "|";

                if (criteria.TrademarkLinks == "M")
                    systemTypes = systemTypes + criteria.TrademarkLinks + "|";

                if (criteria.GeneralMatter == "G")
                    systemTypes = systemTypes + criteria.GeneralMatter + "|";
                
                if (criteria.DMS == "D")
                    systemTypes = systemTypes + criteria.DMS + "|";
                
                if (criteria.AMS == "A")
                   systemTypes = systemTypes + criteria.AMS + "|";
            return systemTypes;
        }


        private string GetSystemTypes(List<QueryFilterViewModel> filters)
        {
            var systemTypes = "|";
            foreach (var filter in filters)
            {
                if(filter.Property == "Patent")
                    systemTypes = systemTypes + filter.Value + "|";

                if (filter.Property == "PTOActions")
                    systemTypes = systemTypes + filter.Value + "|";

                if (filter.Property == "Trademark")
                    systemTypes = systemTypes + filter.Value + "|";

                if (filter.Property == "TrademarkLinks")
                    systemTypes = systemTypes + filter.Value + "|";

                if (filter.Property == "GeneralMatter")
                    systemTypes = systemTypes + filter.Value + "|";
                
                if (filter.Property == "DMS")
                    systemTypes = systemTypes + filter.Value + "|";
                
                if (filter.Property == "AMS")
                    systemTypes = systemTypes + filter.Value + "|";
            }
           
            return systemTypes.Substring(0, systemTypes.Length - 1);
        }


        private void ComputeDates(QuickDocketDefaultSettingsViewModel defaultSettings, ref QuickDocketSearchCriteriaViewModel searchCriteria)
        {
            if (defaultSettings.DueDateRange == "Fix")
            {
                var fixDueDates = GetFixDates(defaultSettings.DueDateTimeFrame);
                searchCriteria.FromDueDate = fixDueDates.StartDate;
                searchCriteria.ToDueDate = fixDueDates.EndDate;
            }
            else
            {
                var relativeDueDates = GetRelativeDates(defaultSettings,"DD");
                searchCriteria.FromDueDate = relativeDueDates.StartDate;
                searchCriteria.ToDueDate = relativeDueDates.EndDate;
            }

            if (defaultSettings.BaseDateRange == "None")
            {
                searchCriteria.FromBaseDate = null;
                searchCriteria.ToBaseDate = null;
            }
            else if (defaultSettings.BaseDateRange == "Fix")
            {
                var fixBaseDates = GetFixDates(defaultSettings.BaseDateTimeFrame);
                searchCriteria.FromBaseDate = fixBaseDates.StartDate;
                searchCriteria.ToBaseDate = fixBaseDates.EndDate;
            }
            else
            {
                var relativeBaseDates = GetRelativeDates(defaultSettings,"BD");
                searchCriteria.FromBaseDate = relativeBaseDates.StartDate;
                searchCriteria.ToBaseDate = relativeBaseDates.EndDate;
            }

            if (defaultSettings.DeDocketInstrxDateRange == "None")
            {
                searchCriteria.FromInstructionDate = null;
                searchCriteria.ToInstructionDate = null;
            }
            else if (defaultSettings.DeDocketInstrxDateRange == "Fix")
            {
                var fixDeDocketInstrxDates = GetFixDates(defaultSettings.DeDocketInstrxDateTimeFrame);
                searchCriteria.FromInstructionDate = fixDeDocketInstrxDates.StartDate;
                searchCriteria.ToInstructionDate = fixDeDocketInstrxDates.EndDate;
            }
            else
            {
                var relativeDeDocketInstrxDates = GetRelativeDates(defaultSettings, "ID");
                searchCriteria.FromInstructionDate = relativeDeDocketInstrxDates.StartDate;
                searchCriteria.ToInstructionDate = relativeDeDocketInstrxDates.EndDate;
            }
        }


        private StartAndEndDateDTO GetRelativeDates(QuickDocketDefaultSettingsViewModel settings,string dateType)
        {
            var startDateOperator = dateType=="DD" ? settings.DueDateStartOp : dateType == "BD" ? settings.BaseDateStartOp :settings.DeDocketInstrxDateStartOp;
            var startDateUnit = dateType == "DD" ? settings.DueDateStartUnit : dateType == "BD" ? settings.BaseDateStartUnit : settings.DeDocketInstrxDateStartUnit;
            var startDateOffSet = Convert.ToInt32(dateType == "DD" ? settings.DueDateStartOffset : dateType == "BD" ? settings.BaseDateStartOffset : settings.DeDocketInstrxDateStartOffset);

            var endDateUnit = dateType == "DD" ? settings.DueDateEndUnit : dateType == "BD" ? settings.BaseDateEndUnit : settings.DeDocketInstrxDateEndUnit;
            var endDateOffSet = Convert.ToInt32(dateType == "DD" ? settings.DueDateEndOffset : dateType == "BD" ? settings.BaseDateEndOffset : settings.DeDocketInstrxDateEndOffset);

            if (startDateOperator == "-")
                startDateOffSet = -System.Math.Abs(startDateOffSet);

            var startAndEndDate = new StartAndEndDateDTO();
            startAndEndDate.StartDate = ComputeStartDate(startDateUnit, startDateOffSet);
            startAndEndDate.EndDate = ComputeEndDate(startAndEndDate.StartDate, endDateUnit, endDateOffSet);

            return startAndEndDate;
        }

        private DateTime ComputeStartDate(string startDateUnit, int startDateOffSet)
        {
            var startDate = DateTime.MinValue;

            if (startDateUnit == "D")
            {
                startDate = DateTime.Today.AddDays(startDateOffSet);
            }
            else if (startDateUnit == "W")
            {
                //var weekOfYear = GetWeekNumber(DateTime.Today); // 1 through 53
                startDate = DateTime.Today.AddDays(startDateOffSet * 7);
            }
            else if (startDateUnit == "M")
            {
                startDate = DateTime.Today.AddMonths(startDateOffSet);
            }
            else if (startDateUnit == "Y")
            {
                startDate = DateTime.Today.AddYears(startDateOffSet);
            }

            return startDate;
        }

        private static DateTime ComputeEndDate(DateTime startDate, string endDateUnit, int endDateOffSet)
        {
            var endDate = DateTime.MinValue;

            if (endDateUnit == "D")
            {
                endDate = startDate.AddDays(endDateOffSet);
            }
            else if (endDateUnit == "W")
            {
                endDate = startDate.AddDays(endDateOffSet * 7);
            }
            else if (endDateUnit == "M")
            {
                endDate = startDate.AddMonths(endDateOffSet);
            }
            else if (endDateUnit == "Y")
            {
                endDate = startDate.AddYears(endDateOffSet);
            }

            return endDate;
        }

        private StartAndEndDateDTO GetFixDates(string timeFrame)
        {
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MinValue;
            var startAndEndDate = new StartAndEndDateDTO();

            if (timeFrame == "W")
            {
                startDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
                startAndEndDate.StartDate = startDate;
                startAndEndDate.EndDate = startDate.AddDays(6);
            }
            else if (timeFrame == "M")
            {
                startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                startAndEndDate.StartDate = startDate;
                startAndEndDate.EndDate = startDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                startAndEndDate.StartDate = new DateTime(DateTime.Today.Year, 1, 1);
                startAndEndDate.EndDate = new DateTime(DateTime.Today.Year, 12, 31);
            }

            return startAndEndDate;
        }

        private int GetWeekNumber(DateTime now)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            int weekNumber = ci.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNumber;
        }

        //private string GetDefaultSystem(string sys)
        //{
        //    if (IsRTSEnabled())
        //        return "L";

        //    else if (IsTLEnabled())
        //        return "M";
        //    else
        //        return sys;
        //}

        //private bool IsRTSEnabled()
        //{
        //    return _optionsService.GetOption("PMS", "IsRTS_ON").BooleanValue;
        //}

        //private bool IsTLEnabled()
        //{
        //    return _optionsService.GetOption("TMS", "IsTL_ON").BooleanValue;
        //}

        public QuickDocketPrintViewModel GetQuickDocketSearchCriteria(QuickDocketSearchCriteriaViewModel searchCriteriaViewModel)
        {
            var criteria = _mapper.Map<QuickDocketSearchCriteriaDTO>(searchCriteriaViewModel);
            criteria.SystemTypes = GetSystemTypes(searchCriteriaViewModel);
            var criteria2 = _mapper.Map<QuickDocketPrintViewModel>(criteria);
            criteria2.ReportFormat = 0;

            return criteria2;
        }
    }
}
