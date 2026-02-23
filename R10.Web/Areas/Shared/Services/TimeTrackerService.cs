using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces.Shared;
using R10.Web.Models;

namespace R10.Web.Areas.Shared.Services
{
    public class TimeTrackerService : ITimeTrackerService
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IAttorneyService _attorneyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IParentEntityService<Attorney, TimeTracker> _attorneyTimeTrackerService;
        private readonly IMapper _mapper;
        protected readonly IInventionService _inventionService;
        protected readonly ICountryApplicationService _applicationService;
        protected readonly ITmkTrademarkService _trademarkService;
        protected readonly IGMMatterService _gmMatterService;
        private readonly IMultipleEntityService<GMMatter, GMMatterAttorney> _matterAttorneyService;
        private readonly IEntityService<TimeTrack> _timeTrackEntityService;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ICPiUserEntityFilterRepository _userEntityFilterStore;

        public TimeTrackerService(
            IStringLocalizer<SharedResource> localizer,
            IHttpContextAccessor httpContextAccessor,
            IAttorneyService attorneyService,
            IParentEntityService<Attorney, TimeTracker> attorneyTimeTrackerService,
            IMapper mapper,
            IInventionService inventionService,
            ICountryApplicationService applicationService,
            ITmkTrademarkService trademarkService,
            IGMMatterService gmMatterService,
            IMultipleEntityService<GMMatter, GMMatterAttorney> matterAttorneyService,
            IEntityService<TimeTrack> timeTrackEntityService,
            ISystemSettings<DefaultSetting> settings,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings,
            ICPiUserEntityFilterRepository userEntityFilterStore)
        {
            _localizer = localizer;
            _httpContextAccessor = httpContextAccessor;
            _attorneyService = attorneyService;
            _attorneyTimeTrackerService = attorneyTimeTrackerService;
            _mapper = mapper;
            _inventionService = inventionService;
            _applicationService = applicationService;
            _trademarkService = trademarkService;
            _gmMatterService = gmMatterService;
            _matterAttorneyService = matterAttorneyService;
            _timeTrackEntityService = timeTrackEntityService;
            _settings = settings;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _userEntityFilterStore = userEntityFilterStore;
        }

        public async Task<List<TimeTrackAttorney>> GetStartTimeTrackAttorneys(int id, string systemType)
        {
            DateTime now = DateTime.Now;
            var AttorneyEntities = _attorneyService.QueryableListWithoutFilter;
            var Attorneys = new List<TimeTrackAttorney>();
            int attorneyId = 0;
            var user = _httpContextAccessor.HttpContext.User;
            if (user.GetUserType() == Core.Identity.CPiUserType.Attorney)
            {
                var entityFilter = _userEntityFilterStore.GetUserEntityFilters(user.GetUserIdentifier()).Result.FirstOrDefault();
                if (entityFilter != null)
                {
                    attorneyId = entityFilter.EntityId;
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == attorneyId);
                    if(attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer["Current User"].Value,
                            AttorneyId = attorneyId,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = true
                        };
                        Attorneys.Add(attorney);
                    }
                }

            }
            else
            {
                var attorney = new TimeTrackAttorney()
                {
                    AttorneyPosition = _localizer["Current User"].Value,
                    AttorneyId = attorneyId,
                    UserId = user.GetUserIdentifier(),
                    UserName = user.GetFullName(),
                    Default = true
                };
                Attorneys.Add(attorney);
            }
            if (systemType.Equals("P"))
            {
                var ca = _applicationService.CountryApplications.FirstOrDefault(c => c.AppId == id);
                var inv = _inventionService.QueryableList.FirstOrDefault(c => c.InvId == ca.InvId);
                var defaultBillingAttorney = _patSettings.GetSetting().Result.DefaultBillingAttorney;
                var settings = await _patSettings.GetSetting();

                if (inv.Attorney1ID != null && !Attorneys.Any( c=> c.AttorneyId == inv.Attorney1ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == inv.Attorney1ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney1].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 1
                        };
                        Attorneys.Add(attorney);
                    }
                }

                if (inv.Attorney2ID != null && !Attorneys.Any(c => c.AttorneyId == inv.Attorney2ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == inv.Attorney2ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney2].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 2
                        };
                        Attorneys.Add(attorney);
                    }
                }

                if (inv.Attorney3ID != null && !Attorneys.Any(c => c.AttorneyId == inv.Attorney3ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == inv.Attorney3ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney3].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 3
                        };
                        Attorneys.Add(attorney);
                    }
                }

                if (inv.Attorney4ID != null && !Attorneys.Any(c => c.AttorneyId == inv.Attorney4ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == inv.Attorney4ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney4].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 4
                        };
                        Attorneys.Add(attorney);
                    }
                }

                if (inv.Attorney5ID != null && !Attorneys.Any(c => c.AttorneyId == inv.Attorney5ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == inv.Attorney5ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney5].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 5
                        };
                        Attorneys.Add(attorney);
                    }
                }
            }
            else if (systemType.Equals("T"))
            {
                var tmk = _trademarkService.TmkTrademarks.FirstOrDefault(c => c.TmkId == id);
                var defaultBillingAttorney = _patSettings.GetSetting().Result.DefaultBillingAttorney;
                var settings = await _tmkSettings.GetSetting();

                if (tmk.Attorney1ID != null && !Attorneys.Any(c => c.AttorneyId == tmk.Attorney1ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == tmk.Attorney1ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney1].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 1
                        };
                        Attorneys.Add(attorney);
                    }
                }

                if (tmk.Attorney2ID != null)
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == tmk.Attorney2ID);
                    if (attorneyEntity != null && !Attorneys.Any(c => c.AttorneyId == tmk.Attorney2ID))
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney2].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 2
                        };
                        Attorneys.Add(attorney);
                    }
                }

                if (tmk.Attorney3ID != null && !Attorneys.Any(c => c.AttorneyId == tmk.Attorney3ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == tmk.Attorney3ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney3].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 3
                        };
                        Attorneys.Add(attorney);
                    }
                }

                if (tmk.Attorney4ID != null && !Attorneys.Any(c => c.AttorneyId == tmk.Attorney4ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == tmk.Attorney4ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney4].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 4
                        };
                        Attorneys.Add(attorney);
                    }
                }

                if (tmk.Attorney5ID != null && !Attorneys.Any(c => c.AttorneyId == tmk.Attorney5ID))
                {
                    var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == tmk.Attorney5ID);
                    if (attorneyEntity != null)
                    {
                        var attorney = new TimeTrackAttorney()
                        {
                            AttorneyPosition = _localizer[settings.LabelAttorney5].Value,
                            AttorneyId = attorneyEntity.AttorneyID,
                            AttorneyCode = attorneyEntity.AttorneyCode,
                            AttorneyName = attorneyEntity.AttorneyName,
                            Default = defaultBillingAttorney == 5
                        };
                        Attorneys.Add(attorney);
                    }
                }
            }
            else if (systemType.Equals("G"))
            {
                var gm = _gmMatterService.QueryableList.FirstOrDefault(c => c.MatId == id);
                var gmAttorneys = _matterAttorneyService.QueryableList.Where(c => c.MatId == gm.MatId).OrderBy(c => c.OrderOfEntry);
                if(gmAttorneys.Any())
                {
                    var gmDefaultAttorneyId = gmAttorneys.Any(c => c.AttorneyID == attorneyId) ? attorneyId : gmAttorneys.FirstOrDefault().AttorneyID;
                    var settings = await _settings.GetSetting();
                    foreach (var gmAttorney in gmAttorneys)
                    {
                        if(!Attorneys.Any(c => c.AttorneyId == gmAttorney.AttorneyID))
                        {
                            var attorneyEntity = AttorneyEntities.FirstOrDefault(c => c.AttorneyID == gmAttorney.AttorneyID);
                            if (attorneyEntity != null)
                            {
                                var attorney = new TimeTrackAttorney()
                                {
                                    AttorneyPosition = _localizer[settings.LabelAttorney].Value,
                                    AttorneyId = gmAttorney.AttorneyID,
                                    AttorneyCode = attorneyEntity.AttorneyCode,
                                    AttorneyName = attorneyEntity.AttorneyName,
                                    Default = gmAttorney.AttorneyID == gmDefaultAttorneyId
                                };
                                Attorneys.Add(attorney);
                            }
                        }
                    }
                }
            }
            return Attorneys;
        }

        public async Task<bool> StartTimeTrack(int id, string systemType, string[] attorneyIds)
        {
            DateTime now = DateTime.Now;
            var TimeTracks = new List<TimeTrack>();

            foreach(var attorneyId in attorneyIds)
            {
                int attorneyIdInt;
                if(int.TryParse(attorneyId, out attorneyIdInt))
                {
                    TimeTrack timeTrack = new TimeTrack()
                    {
                        SystemType = systemType,
                        AttorneyID = attorneyIdInt,
                        StartDate = now,
                        AppId = systemType.Equals("P") ? id : null,
                        TmkId = systemType.Equals("T") ? id : null,
                        MatId = systemType.Equals("G") ? id : null,
                        UserId = _httpContextAccessor.HttpContext.User.GetUserIdentifier(),
                        CreatedBy = _httpContextAccessor.HttpContext.User.GetUserName(),
                        UpdatedBy = _httpContextAccessor.HttpContext.User.GetUserName(),
                        DateCreated = now,
                        LastUpdate = now
                    };
                    TimeTracks.Add(timeTrack);
                }
                else
                {
                    TimeTrack timeTrack = new TimeTrack()
                    {
                        SystemType = systemType,
                        TrackUserId = attorneyId,
                        StartDate = now,
                        AppId = systemType.Equals("P") ? id : null,
                        TmkId = systemType.Equals("T") ? id : null,
                        MatId = systemType.Equals("G") ? id : null,
                        UserId = _httpContextAccessor.HttpContext.User.GetUserIdentifier(),
                        CreatedBy = _httpContextAccessor.HttpContext.User.GetUserName(),
                        UpdatedBy = _httpContextAccessor.HttpContext.User.GetUserName(),
                        DateCreated = now,
                        LastUpdate = now
                    };
                    TimeTracks.Add(timeTrack);
                }
            }

            await _timeTrackEntityService.Add(TimeTracks);
            return true;
        }

        public async Task<string?> StopTimeTrack()
        {
            DateTime now = DateTime.Now;

            var timeTracks = _timeTrackEntityService.QueryableList.Where(c => c.StopDate == null && c.UserId == _httpContextAccessor.HttpContext.User.GetUserIdentifier());
            var updatedTimeTracks = new List<TimeTrack>();
            var timeTrackers = new List<TimeTracker>();
            var attorneys = new List<Attorney>();
            var caseInfo = "";

            if (timeTracks.Count() > 0)
            {
                var systemType = timeTracks.First().SystemType;
                if (systemType.Equals("P"))
                {
                    var ca = _applicationService.CountryApplications.FirstOrDefault(c => c.AppId == timeTracks.First().AppId);
                    if (ca != null) 
                        caseInfo = ca.CaseNumber + "/" + ca.Country + (string.IsNullOrEmpty(ca.SubCase) ? "" : "/" + ca.SubCase);
                }
                else if (systemType.Equals("T"))
                {
                    var tmk = _trademarkService.TmkTrademarks.FirstOrDefault(c => c.TmkId == timeTracks.First().TmkId);
                    if (tmk != null)
                        caseInfo = tmk.CaseNumber + "/" + tmk.Country + (string.IsNullOrEmpty(tmk.SubCase) ? "" : "/" + tmk.SubCase);
                }
                else if (systemType.Equals("G"))
                {
                    var gm = _gmMatterService.QueryableList.FirstOrDefault(c => c.MatId == timeTracks.First().MatId);
                    if (gm != null)
                        caseInfo = gm.CaseNumber + (string.IsNullOrEmpty(gm.SubCase) ? "" : "/" + gm.SubCase);
                }
            }
            else
                return null;

            caseInfo += " " + _localizer["for"] + " ";
            var currentUserName = _httpContextAccessor.HttpContext.User.GetUserName();
            foreach (var timeTrack in timeTracks)
            {
                timeTrack.StopDate = now;
                //await _timeTrackEntityService.Update(timeTrack);
                decimal duration = (decimal)((TimeSpan)(timeTrack.StopDate - timeTrack.StartDate)).TotalHours;
                var settings = await _settings.GetSetting();
                if (duration <= settings.TimeTrackMinHours)
                {
                    duration = settings.TimeTrackMinHours;
                }
                else if (duration >= settings.TimeTrackMaxHours)
                {
                    duration = settings.TimeTrackMaxHours;
                }
                updatedTimeTracks.Add(timeTrack);
                if (timeTrack.AttorneyID != 0)
                {
                    TimeTracker timeTracker = new TimeTracker()
                    {
                        AttorneyID = timeTrack.AttorneyID,
                        AppId = timeTrack.AppId,
                        TmkId = timeTrack.TmkId,
                        MatId = timeTrack.MatId,
                        SystemType = timeTrack.SystemType,
                        Duration = duration,
                        EntryDate = (DateTime)timeTrack.StopDate
                    };
                    timeTrackers.Add(timeTracker);

                    var attorney = _attorneyService.QueryableListWithoutFilter.FirstOrDefault(c => c.AttorneyID == timeTrack.AttorneyID);
                    if (attorney != null)
                    {
                        attorney.LastUpdate = now;
                        attorney.UpdatedBy = currentUserName;
                        attorneys.Add(attorney);
                        caseInfo += attorney.AttorneyName + ", ";
                    }
                }
                else
                {
                    TimeTracker timeTracker = new TimeTracker()
                    {
                        TrackUserId = timeTrack.TrackUserId,
                        AppId = timeTrack.AppId,
                        TmkId = timeTrack.TmkId,
                        MatId = timeTrack.MatId,
                        SystemType = timeTrack.SystemType,
                        Duration = duration,
                        EntryDate = (DateTime)timeTrack.StopDate
                    };
                    timeTrackers.Add(timeTracker);
                    caseInfo += _httpContextAccessor.HttpContext.User.GetFullName() + ", ";
                }
            }
            await _timeTrackEntityService.Update(updatedTimeTracks);
            await _attorneyTimeTrackerService.ChildService.Update(timeTrackers);
            await _attorneyService.Update(attorneys);

            return caseInfo.Substring(0, caseInfo.Length - 2);
        }
    }
}