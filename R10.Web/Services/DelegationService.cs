using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class DelegationService : IDelegationService
    {
        private readonly UserManager<CPiUser> _userManager;
        private readonly ICPiUserGroupManager _userGroupManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IEmailSender _emailSender;
        private readonly IApplicationDbContext _repository;
        private readonly IRSMainService _rSMainService;
        private readonly IRSActionService _rSActionService;
        private readonly ReportSchedulerHelper _reportSchedulerHelper;
        private readonly IRSCTMService _rSCTMService;

        public DelegationService(
            UserManager<CPiUser> userManager,
            ICPiUserGroupManager userGroupManager,
            IHttpContextAccessor httpContextAccessor,
            ISystemSettings<DefaultSetting> defaultSettings,
            IStringLocalizer<SharedResource> localizer, 
            IEmailSender emailSender,
            IApplicationDbContext repository,
            IRSMainService rSMainService,
            IRSActionService rSActionService,
            IConfiguration configuration,
            IRSCTMService rSCTMService)
        {
            _userManager = userManager;
            _userGroupManager = userGroupManager;
            _httpContextAccessor = httpContextAccessor;
            _defaultSettings = defaultSettings;
            _localizer = localizer;
            _emailSender = emailSender;
            _repository = repository;
            _rSMainService = rSMainService;
            _rSActionService = rSActionService;
            _reportSchedulerHelper = new ReportSchedulerHelper(configuration);
            _rSCTMService = rSCTMService;
        }

        public IQueryable<CPiGroup> GetGroups(string? system = null, int? caseId = null)
        {
            if (system != null && caseId != null)
            {
                var dt = GetAvaliableGroupAndUserTable(system, caseId);
                var groups = _userGroupManager.GetGroups().ToList();
                List<CPiGroup> list = new List<CPiGroup>();
                foreach (var group in groups)
                {
                    if (dt.AsEnumerable().Any(c => group.Id.Equals(c.Field<int?>("GroupId"))))
                        list.Add(group);
                }
                return list.AsQueryable<CPiGroup>();
            }
            return _userGroupManager.GetGroups();
        }

        public IQueryable<CPiUser> GetUsers(string? system = null, int? caseId = null)
        {
            if (system != null && caseId != null)
            {
                var dt = GetAvaliableGroupAndUserTable(system, caseId);       
                var users = _userManager.Users.ToList();
                List<CPiUser> list = new List<CPiUser>();
                foreach (var user in users)
                {
                    if (dt.AsEnumerable().Any(c => user.Id.Equals(c.Field<string?>("UserId"))))
                        list.Add(user);
                }
                return list.AsQueryable<CPiUser>();
            }
            return _userManager.Users;
        }

        private DataTable GetAvaliableGroupAndUserTable(string? system = null, int? caseId = null)
        {
            using (SqlDataAdapter da = new SqlDataAdapter("procWebSysGetAvaliableUsersGroups", GetSqlConnection()))
            {
                DataTable dt = new DataTable();
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@System", system);
                da.SelectCommand.Parameters.AddWithValue("@CaseId", caseId);

                da.Fill(dt);
                return dt;
            }
        }

        public async Task DelegationSetUp()
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            if (defaultSettings.IsDelegationOn && defaultSettings.IsDelegationReportOn)
            {
                // Add Daily Delegation Report
                RSMain? main = _rSMainService.QueryableList.FirstOrDefault(c => c.Name.Equals("Delegation Report"));
                if (main == null)
                {
                    main = await CreateRSMainForDelegation("Delegation Report");
                    await _rSMainService.Add(main);
                    RSAction action = CreateRSActionForDelegation(main);
                    await _rSActionService.Add(action);
                }
                if (main.CreatedBy == "pending")
                {
                    tblCTMMain cTMMain = await CreateCTMEntity(main);
                    try
                    {
                        bool result = _rSCTMService.InsertCTMSchedule(cTMMain);
                        if (result)
                        {
                            main.CreatedBy = "sa";
                            await _rSMainService.Update(main);
                        }
                    }
                    catch (Exception)
                    {

                    }
                    
                }

                // Add Reminder Delegation Report
                RSMain? mainReminder = _rSMainService.QueryableList.FirstOrDefault(c => c.Name.Equals("Delegation Report Reminder"));
                if (mainReminder == null)
                {
                    mainReminder = await CreateRSMainForDelegation("Delegation Report Reminder");
                    await _rSMainService.Add(mainReminder);
                    RSAction action = CreateRSActionForDelegation(mainReminder);
                    await _rSActionService.Add(action);
                }
                if (mainReminder.CreatedBy == "pending")
                {
                    tblCTMMain cTMMainReminder = await CreateCTMEntity(mainReminder);
                    try
                    {
                        bool result = _rSCTMService.InsertCTMSchedule(cTMMainReminder);
                        if (result)
                        {
                            mainReminder.CreatedBy = "sa";
                            await _rSMainService.Update(mainReminder);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else
            {
                // Remove Daily Delegation Report
                RSMain? main = _rSMainService.QueryableList.FirstOrDefault(c => c.Name.Equals("Delegation Report"));
                if (main != null)
                {
                    if (main.CreatedBy == "sa")
                    {
                        tblCTMMain cTMMain = await CreateCTMEntity(main);
                        bool result = _rSCTMService.DeleteCTMSchedule(cTMMain);
                        if (result)
                        {
                            RSAction? actionDelete = _rSActionService.QueryableList.FirstOrDefault(c=>c.TaskId == main.TaskId);
                            if (actionDelete != null)
                                await _rSActionService.Delete(actionDelete);
                            await _rSMainService.Delete(main);
                        }
                    }
                    else
                    {
                        RSAction? actionDelete = _rSActionService.QueryableList.FirstOrDefault(c => c.TaskId == main.TaskId);
                        if (actionDelete != null)
                            await _rSActionService.Delete(actionDelete);
                        await _rSMainService.Delete(main);
                    }
                }

                // Remove Reminder Delegation Report
                RSMain? mainReminder = _rSMainService.QueryableList.FirstOrDefault(c => c.Name.Equals("Delegation Report Reminder"));
                if (mainReminder != null)
                {
                    if (mainReminder.CreatedBy == "sa")
                    {
                        tblCTMMain cTMMainReminder = await CreateCTMEntity(mainReminder);
                        bool result = _rSCTMService.DeleteCTMSchedule(cTMMainReminder);
                        if (result)
                        {
                            RSAction? actionDelete = _rSActionService.QueryableList.FirstOrDefault(c => c.TaskId == mainReminder.TaskId);
                            if (actionDelete != null)
                                await _rSActionService.Delete(actionDelete);
                            await _rSMainService.Delete(mainReminder);
                        }
                    }
                    else
                    {
                        RSAction? actionDelete = _rSActionService.QueryableList.FirstOrDefault(c => c.TaskId == mainReminder.TaskId);
                        if (actionDelete != null)
                            await _rSActionService.Delete(actionDelete);
                        await _rSMainService.Delete(mainReminder);
                    }
                }
            }
        }

        private async Task<RSMain> CreateRSMainForDelegation(string name)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            string time = defaultSettings.DelegationReportSendingTime;
            if (time == null)
                time = "0:0 AM";
            int hours = int.Parse(time.Substring(0, time.IndexOf(":")))+(time.Substring(time.Length - 2).ToLower().Equals("pm")?12:0);
            int minutes = int.Parse(time.Substring(time.IndexOf(":") + 1, time.Length - time.IndexOf(":") - 4));

            DateTime startRunningTime = DateTime.Now.Date;
            startRunningTime = startRunningTime.AddHours(hours);
            startRunningTime = startRunningTime.AddMinutes(minutes);

            DateTime nextRunningTime = startRunningTime;
            if(!name.Equals("Delegation Report"))
            {
                DayOfWeek currentDayOfWeek = nextRunningTime.DayOfWeek;
                if (((int)currentDayOfWeek) <= 1)
                {
                    nextRunningTime.AddDays(1 - ((int)currentDayOfWeek));
                }
                else
                {
                    nextRunningTime.AddDays(8 - ((int)currentDayOfWeek));
                }

            }

            RSMain main = new RSMain() { 
                ReportId=7,
                Name= name,
                Description="Report for " + (name.Equals("Delegation Report") ? "daily" : "reminder") + " delegation notification task, Do not Delete or Edit",
                IsEnabled=true,
                NextRunTime= nextRunningTime,
                CreatedBy = "pending",
                UpdatedBy = "sa",
                DateCreated = DateTime.Now,
                LastUpdate = DateTime.Now,
                FreqTypeId = name.Equals("Delegation Report")?1:2,
                TaskStartDateTime= startRunningTime,
                Sun= name.Equals("Delegation Report"),
                Mon=true,
                Tue= name.Equals("Delegation Report"),
                Wed = name.Equals("Delegation Report"),
                Thu = name.Equals("Delegation Report"),
                Fri = name.Equals("Delegation Report"),
                Sat = name.Equals("Delegation Report"),
                DayOfMonth ="First",
                DateType="Due Date",
                IsFixedRange="0",
                StartDateOperator="-",
                StartDateOffSet=0,
                StartDateUnit="0",
                EndDateOffSet=0,
                EndDateUnit="0",
                FixedRange="0",
                TaskCreatorId="",
                IsShared=false,
                IsEditable=false,
            };
            return main;
        }

        private RSAction CreateRSActionForDelegation(RSMain main)
        {
            RSAction action = new RSAction()
            {
                TaskId = main.TaskId,
                ActionTypeId = 1,
                OutputFormat = "PDF",
                SortOrder = "",
                CreatedBy = "sa",
                UpdatedBy = "sa",
                DateCreated = DateTime.Now,
                LastUpdate = DateTime.Now,
                IsEnabled = true,
                EmailSubject = main.Name,
                EmailBody = "Please see attachment."
            };
            return action;
        }

        private async Task<tblCTMMain> CreateCTMEntity(RSMain rSMain)
        {
            var defaultSettings = await _defaultSettings.GetSetting();

            tblCTMMain schedule = new tblCTMMain();

            schedule.SchedID = rSMain.TaskId;
            schedule.TaskCode = defaultSettings.RSCTMTaskCode.Replace("-RS", "-DE");
            schedule.TaskName = defaultSettings.RSCTMClientName + " " + rSMain.Name;
            schedule.Active = rSMain.IsEnabled;
            schedule.NextProcessDate = rSMain.NextRunTime;
            schedule.WorkStationID = defaultSettings.DECTMTaskCode;
            schedule.Notes = await GetDelegationTrigger();
            schedule.URL = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host.Value.Replace("-staging", "") + _httpContextAccessor.HttpContext.Request.PathBase;
            return CreateSchedule(schedule);
        }

        private tblCTMMain CreateSchedule(tblCTMMain reportSchedule)
        {
            tblCTMMain schedule = new tblCTMMain();
            //schedule.UniqueID = reportSchedule.UniqueID;
            schedule.SchedID = reportSchedule.SchedID;
            schedule.TaskCode = reportSchedule.TaskCode; //<First 17 LETTERS of the client name from tblPubOptions, no spaces, no special characters> + 
            schedule.TaskName = reportSchedule.TaskName; //<First 17 LETTERS of the client name from tblPubOptions, no spaces, no special characters> + " RS " + <Upto 29 character report description>
            schedule.TaskType = 7; //Delegation
            schedule.SQLServer = _reportSchedulerHelper.GetServerName();
            schedule.DBName = _reportSchedulerHelper.GetDatabaseName();
            schedule.DBConfigName = _reportSchedulerHelper.GetDatabaseName();
            schedule.Active = reportSchedule.Active;
            schedule.NeedsRefresh = true; //<Set to TRUE if record just added or edited>
            schedule.WorkStationID = reportSchedule.WorkStationID;
            schedule.Notes = reportSchedule.Notes;
            schedule.URL = reportSchedule.URL;
            schedule.NextProcessDate = reportSchedule.NextProcessDate;
            return schedule;
        }

        public async Task<string> GetDelegationTrigger()
        {
            var defaultSettings = await _defaultSettings.GetSetting();

            string taskStartDateString = DateTime.Now.ToString("dd-MMM-yyyy");
            string taskStartTimeString = defaultSettings.DelegationReportSendingTime;

            return String.Format("At {0} every day, starting {1}", taskStartTimeString, taskStartDateString);
        }

        public List<DelegateUser> GetDelegateUsers(DelegationParameter parameter)
        {
            List<DelegateUser> users = new List<DelegateUser>();
            using (SqlCommand cmd = new SqlCommand("procWebSysDueDateDelegationReport"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = GetSqlConnection();
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@ActiveSwitch", parameter.ActiveSwitch);
                cmd.Parameters.AddWithValue("@PrintSystems", parameter.PrintSystems);
                cmd.Parameters.AddWithValue("@StartDateAdjustment", parameter.StartDateAdjustment);
                cmd.Parameters.AddWithValue("@ReminderToDateAdjustment", parameter.ReminderToDateAdjustment);
                cmd.Parameters.AddWithValue("@TaskId", parameter.TaskId);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["UserId"] != DBNull.Value)
                    {
                        users.Add(ConvertToDelegateUser(reader));
                    }
                }
                return users;
            }
        }

        private DelegateUser ConvertToDelegateUser(SqlDataReader reader)
        {
            return new DelegateUser
            {
                UserID = (string)reader["UserId"],
                Email = (string)reader["Email"],
            };
        }

        public void UpdateNotificationSent(DelegationParameter parameter)
        {
            List<DelegateUser> users = new List<DelegateUser>();
            using (SqlCommand cmd = new SqlCommand("procWebSysDelegationNotificationSentUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = GetSqlConnection();
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@ActiveSwitch", parameter.ActiveSwitch);
                cmd.Parameters.AddWithValue("@PrintSystems", parameter.PrintSystems);

                cmd.ExecuteNonQuery();
            }
        }

        private SqlConnection GetSqlConnection()
        {
            return new SqlConnection(_repository.Database.GetDbConnection().ConnectionString);
        }
    }
}
