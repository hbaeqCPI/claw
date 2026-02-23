using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Data;
using System.Data.SqlClient;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.Patent;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace R10.Core.Services.GeneralMatter
{
    public class GMMatterService : EntityService<GMMatter>, IGMMatterService
    {
        private readonly ISystemSettings<GMSetting> _settings;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        //USE ICPiDbContext
        private readonly IApplicationDbContext _repository;

        public GMMatterService(
            ICPiDbContext cpiDbContext,
            IApplicationDbContext repository,
            ClaimsPrincipal user,
            ISystemSettings<GMSetting> settings,
            ICPiSystemSettingManager systemSettingManager) : base(cpiDbContext, user)
        {
            _settings = settings;
            _repository = repository;
            _systemSettingManager = systemSettingManager;
        }

        public override IQueryable<GMMatter> QueryableList
        {
            get
            {
                var matters = _cpiDbContext.GetRepository<GMMatter>().QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.GeneralMatter))
                    matters = matters.Where(RespOfficeFilter());

                if (_user.HasEntityFilter())
                    matters = matters.Where(EntityFilter());

                return matters;
            }
        }

        public IQueryable<T> QueryableChildList<T>() where T : BaseEntity
        {
            var queryableList = _repository.Set<T>() as IQueryable<T>;

            if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                queryableList = queryableList.Where(a => this.QueryableList.Any(gm => gm.MatId == EF.Property<int>(a, "MatId")));

            return queryableList;
        }

        //public IQueryable<GMMatterCopySetting> GMMatterCopySettings => _repository.GMMatterCopySettings.AsNoTracking();
        public IQueryable<GMMatterCopySetting> GMMatterCopySettings => _cpiDbContext.GetRepository<GMMatterCopySetting>().QueryableList;
        public bool IsAttorneyRequired => _user.GetEntityFilterType() == CPiEntityType.Attorney;

        private Expression<Func<GMMatter, bool>> RespOfficeFilter()
        {
            return a => CPiUserSystemRoles.Any(r => r.UserId == UserId && r.SystemId == SystemType.GeneralMatter && a.RespOffice == r.RespOffice && !string.IsNullOrEmpty(r.RespOffice));
        }

        private Expression<Func<GMMatter, bool>> EntityFilter()
        {
            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return gm => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == gm.ClientID);

                case CPiEntityType.Agent:
                    return gm => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == gm.AgentID);

                case CPiEntityType.Attorney:
                    return gm => UserEntityFilters.Any(f => f.UserId == UserId && gm.Attorneys.Any(att => att.AttorneyID == f.EntityId));
            }
            return gm => true;
        }

        public override async Task<GMMatter> GetByIdAsync(int matId)
        {
            return await QueryableList.SingleOrDefaultAsync(gm => gm.MatId == matId);
        }

        public async Task Add(GMMatter matter, List<int> attorneyIds)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.GeneralMatter, CPiPermissions.FullModify, matter.RespOffice));

            if (IsAttorneyRequired)
                Guard.Against.Null(attorneyIds?.Count > 0 ? attorneyIds : null, "Attorney");

            if (matter.MatterStatusDate == null) matter.MatterStatusDate = DateTime.Today;

            await ValidateMatter(matter);

            _cpiDbContext.GetRepository<GMMatter>().Add(matter);
            if (IsAttorneyRequired)
            {
                var matterAttorneys = attorneyIds.Select((attorneyId, orderOfEntry) => 
                    new GMMatterAttorney
                    {
                        AttorneyID = attorneyId,
                        MatId = matter.MatId,
                        OrderOfEntry = orderOfEntry,
                        CreatedBy = matter.CreatedBy,
                        DateCreated = matter.DateCreated,
                        UpdatedBy = matter.UpdatedBy,
                        LastUpdate = matter.LastUpdate
                    }).ToList();
                _cpiDbContext.GetRepository<GMMatterAttorney>().Add(matterAttorneys);
            }
            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Add(GMMatter matter)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.GeneralMatter, CPiPermissions.FullModify, matter.RespOffice));

            if (IsAttorneyRequired)
                Guard.Against.Null(null, "Attorney");

            if (matter.MatterStatusDate == null) matter.MatterStatusDate = DateTime.Today;

            await ValidateMatter(matter);
            await base.Add(matter);
        }

        public async Task CopyMatter(int oldMatId, int newMatId, string userName, bool copyCaseInfo,
            bool CopyCountries, bool CopyAttorney, bool CopyOtherParties, bool CopyTrademarks,
            bool CopyPatents, bool CopyKeywords, bool CopyImages, bool CopyRelatedCases, bool CopyProducts)
        {
            await _cpiDbContext.ExecuteSqlRawAsync($"procGmMatterCopy @OldMatId={oldMatId},@NewMatId={newMatId},@CreatedBy='{userName}',@CopyCaseInfo={copyCaseInfo},@CopyCountries={CopyCountries},@CopyAttorney={CopyAttorney},@CopyOtherParties={CopyOtherParties},@CopyTrademarks={CopyTrademarks},@CopyPatents={CopyPatents},@CopyKeywords={CopyKeywords},@CopyImages={CopyImages},@CopyRelatedCases={CopyRelatedCases},@CopyProducts={CopyProducts}");
        }
        public async Task AddCustomFieldsAsCopyFields()
        {
            await _cpiDbContext.ExecuteSqlRawAsync(@$"Insert Into tblGMMatterCopySetting(FieldDesc,FieldName,[Copy],UserName)
                                                 Select cfs.ColumnLabel,cfs.ColumnName,0,cs.UserName from tblSysCustomFieldSetting cfs 
                                                 Cross Join(Select Distinct UserName From tblGMMatterCopySetting) cs
                                                 Where cfs.TableName='tblGMMatter' and cfs.Visible = 1 
                                                 And Not Exists(Select 1 From tblGMMatterCopySetting ecs Where ecs.FieldName=cfs.ColumnName and isnull(ecs.UserName,'')=isnull(cs.UserName,''))
                                                 Order By cfs.OrderOfEntry");
            
            await _cpiDbContext.ExecuteSqlRawAsync(@$"Delete ecs From tblGMMatterCopySetting ecs 
                    Where ecs.FieldName like 'CustomField%' And FieldName Not In(Select cfs.ColumnName from tblSysCustomFieldSetting cfs 
                    Where cfs.TableName='tblGMMatter' and cfs.Visible = 1) ");
        }


        public override async Task Update(GMMatter matter)
        {
            await ValidatePermission(matter.MatId, CPiPermissions.FullModify);
            await ValidateMatter(matter);

            var oldMatter = await GetByIdAsync(matter.MatId);
            if (oldMatter.MatterStatus != matter.MatterStatus && oldMatter.MatterStatusDate == matter.MatterStatusDate)
            {
                matter.MatterStatusDate = DateTime.Today;
            }

            await base.Update(matter);
        }

        public override async Task UpdateRemarks(GMMatter entity)
        {
            await ValidatePermission(entity.MatId, CPiPermissions.RemarksOnly);
            await base.UpdateRemarks(entity);
        }

        public override async Task Delete(GMMatter matter)
        {
            await ValidatePermission(matter.MatId, CPiPermissions.CanDelete);
            await base.Delete(matter);
        }

        public async Task ValidatePermission(int matId, List<string> roles)
        {
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.GeneralMatter))
            {
                var item = (await QueryableList.Where(i => i.MatId == matId).Select(i => new { i.MatId, i.RespOffice }).ToDictionaryAsync(i => i.MatId, i => i.RespOffice)).FirstOrDefault();
                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.GeneralMatter, roles, respOfc));
        }

        public async Task<List<GMWorkflowAction>> CheckWorkflowAction(GMWorkflowTriggerType triggerType)
        {
            var actions = await _cpiDbContext.GetRepository<GMWorkflowAction>().QueryableList.Where(w => w.Workflow.TriggerTypeId == (int)triggerType && w.Workflow.ActiveSwitch).Include(w => w.Workflow).ThenInclude(w => w.SystemScreen).OrderBy(w => w.OrderOfEntry).ToListAsync();
            return actions;
        }

        private async Task ValidateMatter(GMMatter matter, List<string>? roles = null)
        {
            var entityFilterType = _user.GetEntityFilterType();
            var settings = await _settings.GetSetting();

            if (entityFilterType == CPiEntityType.Client)
            {
                Guard.Against.Null(matter.ClientID, settings.LabelClient);
                Guard.Against.ValueNotAllowed(await EntityFilterAllowed(matter.ClientID ?? 0), settings.LabelClient);
            }

            if (entityFilterType == CPiEntityType.Agent)
            {
                Guard.Against.Null(matter.AgentID, settings.LabelAgent);
                Guard.Against.ValueNotAllowed(await EntityFilterAllowed(matter.AgentID ?? 0), settings.LabelAgent);
            }

            if (_user.IsRespOfficeOn(SystemType.GeneralMatter))
            {
                Guard.Against.Null(matter.RespOffice, "Responsible Office");
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.GeneralMatter, matter.RespOffice, roles), "Responsible Office");
            }
        }

        public async Task RefreshCopySetting(List<GMMatterCopySetting> added, List<GMMatterCopySetting> deleted)
        {
            if (added.Count > 0)
                //_repository.GMMatterCopySettings.AddRange(added);
                _cpiDbContext.GetRepository<GMMatterCopySetting>().Add(added);

            if (deleted.Count > 0)
                //_repository.GMMatterCopySettings.RemoveRange(deleted);
                _cpiDbContext.GetRepository<GMMatterCopySetting>().Delete(deleted);
            //await _repository.SaveChangesAsync();
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateCopySetting(GMMatterCopySetting setting)
        {
            //var existing = await _repository.GMMatterCopySettings.FirstOrDefaultAsync(s => s.CopySettingId == setting.CopySettingId);
            var existing = await _cpiDbContext.GetRepository<GMMatterCopySetting>().GetByIdAsync(setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;
                //_repository.GMMatterCopySettings.Update(existing);
                _cpiDbContext.GetRepository<GMMatterCopySetting>().Update(existing);
                //await _repository.SaveChangesAsync();
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task AddCopySettings(List<GMMatterCopySetting> settings)
        {
            if (settings.Count > 0)
            {
                _cpiDbContext.GetRepository<GMMatterCopySetting>().Add(settings);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<CPiUserSetting> GetMainCopySettings(string userId)
        {
            var setting = await _repository.CPiSettings.Where(d => d.Name == "GeneralMatterCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new CPiSetting { Name = "GeneralMatterCopySetting", Policy = "*" };
                _repository.CPiSettings.Add(setting);
                await _repository.SaveChangesAsync();
            }
            return await _repository.CPiUserSettings.Where(u => u.UserId == userId && u.SettingId == setting.Id).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task UpdateMainCopySettings(CPiUserSetting userSetting)
        {
            if (userSetting.Id > 0)
                _repository.CPiUserSettings.Update(userSetting);
            else
                _repository.CPiUserSettings.Add(userSetting);
            await _repository.SaveChangesAsync();
        }

        public async Task<int> GetMainCopySettingId()
        {
            var setting = await _repository.CPiSettings.Where(d => d.Name == "GeneralMatterCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting != null)
            {
                return setting.Id;
            }
            else
            {
                setting = new CPiSetting { Name = "GeneralMatterCopySetting", Policy = "*" };
                _repository.CPiSettings.Add(setting);
                await _repository.SaveChangesAsync();
                return setting.Id;
            }
        }

        public async Task<bool> HasProducts(int matId)
        {
            return await _cpiDbContext.GetRepository<GMProduct>().QueryableList.AnyAsync(p => p.MatId == matId);
        }
        public IQueryable<Product> Products => _repository.Products.AsNoTracking();

        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            return await _repository.SysCustomFieldSettings.Where(s => s.TableName == "tblGMMatter" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }
        public async Task<bool> HasOutstandingDedocket(int matId)
        {
            return await _repository.GMActionsDue.AnyAsync(ad => ad.MatId == matId && ad.DueDates.Any(dd => dd.DeDocketOutstanding != null));
        }

        #region Workflow
        public async Task GenerateWorkflowFromEmailSent(int matId, int qeSetupId)
        {
            var workflowActions = (await CheckWorkflowAction(GMWorkflowTriggerType.EmailSent)).Where(wf => (wf.Workflow.SystemScreen == null || wf.Workflow.SystemScreen.ScreenCode.ToLower() == "gm-workflow") && (wf.Workflow.TriggerValueId == 0 || wf.Workflow.TriggerValueId == qeSetupId)).ToList();
            if (workflowActions.Any())
            {
                var gm = await QueryableList.Where(c => c.MatId == matId).FirstOrDefaultAsync();
                workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || w.Workflow.ClientFilter.Contains("|" + gm.ClientID.ToString() + "|")).ToList();

                if (workflowActions.Any())
                {
                    //client specific will override the base
                    foreach (var item in workflowActions.Where(wf => !string.IsNullOrEmpty(wf.Workflow.ClientFilter)).ToList())
                    {
                        workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && bf.ActionTypeId == item.ActionTypeId);
                    }

                    var createActionWorkflows = workflowActions.Where(wf => wf.ActionTypeId == (int)GMWorkflowActionType.CreateAction).Distinct().ToList();
                    foreach (var item in createActionWorkflows)
                    {
                        await GenerateWorkflowAction(gm.MatId, item.ActionValueId);
                    }
                }
            }
        }

        public async Task GenerateWorkflowFromActionEmailSent(int actId, int qeSetupId)
        {
            var workflowActions = (await CheckWorkflowAction(GMWorkflowTriggerType.EmailSent)).Where(wf => (wf.Workflow.SystemScreen == null || wf.Workflow.SystemScreen.ScreenCode.ToLower() == "act-workflow") && (wf.Workflow.TriggerValueId == 0 || wf.Workflow.TriggerValueId == qeSetupId)).ToList();
            if (workflowActions.Any())
            {
                var actionDue = await _repository.GMActionsDue.Where(a => a.ActId == actId).Include(a => a.GMMatter).FirstOrDefaultAsync();
                workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || w.Workflow.ClientFilter.Contains("|" + actionDue.GMMatter.ClientID.ToString() + "|")).ToList();
                if (workflowActions.Any())
                {
                    //client specific will override the base
                    foreach (var item in workflowActions.Where(wf => !string.IsNullOrEmpty(wf.Workflow.ClientFilter)).ToList())
                    {
                        workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && bf.ActionTypeId == item.ActionTypeId);
                    }

                    var createActionWorkflows = workflowActions.Where(wf => wf.ActionTypeId == (int)GMWorkflowActionType.CreateAction).Distinct().ToList();
                    foreach (var item in createActionWorkflows)
                    {
                        await GenerateWorkflowAction(actionDue.MatId, item.ActionValueId);
                    }
                }
            }
        }

        public async Task GenerateWorkflowAction(int matId, int actionTypeId, DateTime? baseDate = null)
        {
            if (baseDate == null) 
                baseDate = DateTime.Now.Date;

            var matter = await GetByIdAsync(matId);

            var actionType = await _repository.GMActionTypes.Where(at => at.ActionTypeID == actionTypeId).AsNoTracking().FirstOrDefaultAsync();
            if (actionType == null) return;

            var dupActionDue = await _repository.GMActionsDue.Where(a => a.MatId == matId && a.BaseDate.Date == baseDate && a.ActionType == actionType.ActionType).AsNoTracking().FirstOrDefaultAsync();
            if (dupActionDue == null)
            {
                GMActionDue actionDue = new GMActionDue() { MatId = matter.MatId, CaseNumber = matter.CaseNumber, SubCase = matter.SubCase, ActionType = actionType.ActionType, BaseDate = (DateTime)baseDate, ResponsibleID = null, CreatedBy = _user.GetUserName(), UpdatedBy = _user.GetUserName(), DateCreated = DateTime.Now, LastUpdate = DateTime.Now };

                var dueDates = new List<GMDueDate>();
                var actionParams = await _repository.GMActionParameters.Where(ap => ap.ActionTypeID == actionType.ActionTypeID).AsNoTracking().ToListAsync();

                if (actionParams.Any())
                    dueDates = actionParams.Select(ap => new GMDueDate()
                    {
                        ActId = actionDue.ActId,
                        ActionDue = ap.ActionDue,
                        DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                        DateTaken = actionDue.ResponseDate,
                        Indicator = ap.Indicator,
                        CreatedBy = actionDue.UpdatedBy,
                        DateCreated = actionDue.LastUpdate,
                        UpdatedBy = actionDue.UpdatedBy,
                        LastUpdate = actionDue.LastUpdate
                    }).ToList();
                else
                    dueDates.Add(new GMDueDate()
                    {
                        ActId = actionDue.ActId,
                        ActionDue = actionDue.ActionType,
                        DueDate = actionDue.BaseDate,
                        DateTaken = actionDue.ResponseDate,
                        Indicator = "Due Date",
                        CreatedBy = actionDue.UpdatedBy,
                        DateCreated = actionDue.LastUpdate,
                        UpdatedBy = actionDue.UpdatedBy,
                        LastUpdate = actionDue.LastUpdate
                    });
                actionDue.DueDates = dueDates;

                var dueDatesFromIndicatorWorkflow = await GenerateDueDateFromActionParameterWorkflow(actionDue, actionDue.DueDates, GMWorkflowTriggerType.Indicator);
                if (dueDatesFromIndicatorWorkflow != null && dueDatesFromIndicatorWorkflow.Any())
                {
                    actionDue.DueDates.AddRange(dueDatesFromIndicatorWorkflow);
                }

                _repository.GMActionsDue.Add(actionDue);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<List<GMActionDue>> CloseWorkflowAction(int matId, int actionTypeId)
        {
            var matter = await GetByIdAsync(matId);
            var actionDues = new List<GMActionDue>();

            if (actionTypeId != 0)
            {
                var actionType = await _repository.GMActionTypes.Where(at => at.ActionTypeID == actionTypeId).AsNoTracking().FirstOrDefaultAsync();
                if (actionType != null) {
                    //var actionDue = await _repository.GMActionsDue.Where(a => a.MatId == matId && a.ActionType == actionType.ActionType && a.ResponseDate == null).FirstOrDefaultAsync();
                    //if (actionDue != null)
                    //{
                    //    actionDue.ResponseDate = DateTime.Now.Date;
                    //    actionDue.UpdatedBy = _user.GetUserName();
                    //    actionDue.LastUpdate = DateTime.Now;

                    //    var dueDates = await _repository.GMDueDates.Where(d => d.ActId == actionDue.ActId && d.DateTaken == null)
                    //                        .ToListAsync();
                    //    foreach (var dueDate in dueDates)
                    //    {
                    //        dueDate.DateTaken = DateTime.Now.Date;
                    //        dueDate.LastUpdate = actionDue.LastUpdate;
                    //        dueDate.UpdatedBy = actionDue.UpdatedBy;
                    //    }
                    //    await _repository.SaveChangesAsync();
                    //}
                    actionDues = await _repository.GMActionsDue.Where(a => a.MatId == matId && a.ActionType == actionType.ActionType && (a.ResponseDate == null || a.DueDates.Any(dd => dd.DateTaken == null))).Include(a => a.DueDates).AsNoTracking().ToListAsync();
                }
            }
            
            //all outstanding actions
            else if (actionTypeId == 0)
            {
                actionDues = await _repository.GMActionsDue.Where(a => a.MatId == matId && (a.ResponseDate == null || a.DueDates.Any(dd => dd.DateTaken == null))).Include(a => a.DueDates).AsNoTracking().ToListAsync();
            }

            if (actionDues.Any())
            {
                foreach (var actionDue in actionDues)
                {
                    //for all outstanding actions, we want to close everything and avoid followup
                    if (actionTypeId != 0) {
                        if (actionDue.ResponseDate == null)
                        {
                            actionDue.ResponseDate = DateTime.Now.Date;
                            actionDue.UpdatedBy = _user.GetUserName();
                            actionDue.LastUpdate = DateTime.Now;
                        }
                    }
                    actionDue.CloseDueDates = true;

                }
            }
            return actionDues;
        }

        public async Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId)
        {
            var list = await _repository.DelegationEmailDTO.FromSqlInterpolated($@"Select Distinct DelegationId,AssignedBy,AssignedTo,FirstName,LastName From
                                (Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From tblCPiGroups g Inner Join tblCPiUserGroups ug on g.Id=ug.GroupId Inner Join tblCPIUsers u on u.Id=ug.UserId Inner Join tblGMDueDateDelegation ddd on ddd.GroupId=ug.GroupId Union
                                 Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From  tblCPIUsers u  Inner Join tblGMDueDateDelegation ddd on ddd.UserId=u.Id
                                ) t Where t.DelegationId={delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId)
        {
            var delegation = await _repository.DelegationDetailDTO.FromSqlInterpolated($"Select DataKeyValue as DelegationId, dddLog.ActID,dddLog.DDID,dddLog.GroupId,dddLog.UserId,dddLog.NotificationSent,ParentActId,ParentId    From tblDeleteLog d WITH (NOLOCK) cross apply openjson(d.record) With(ActId int '$.ActId',DDId int '$.DDId',GroupId int '$.GroupId',UserId nvarchar(450) '$.UserId',NotificationSent int '$.NotificationSent',ParentActId int '$.ParentActId',ParentId int '$.ParentId') as dddlog Where DataKey='DelegationId' and SystemType='G' And DataKeyValue={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return delegation;
        }

        public async Task MarkDelegationasEmailed(int delegationId)
        {
            await _repository.Database.ExecuteSqlInterpolatedAsync($"Update tblGMDueDateDelegation Set NotificationSent=1 Where DelegationId={delegationId}");
        }

        public async Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds)
        {
            var ids = new DataTable();
            ids.Columns.Add("Id", typeof(Int32));
            ids.Columns.Add("DueDate", typeof(DateTime));

            var result = new List<LookupIntDTO>();
            foreach (var item in recIds)
            {
                var newRec = ids.NewRow();
                newRec["Id"] = item;
                ids.Rows.Add(newRec);
            }

            var connectionString = _repository.Database.GetDbConnection().ConnectionString;
            using (SqlCommand cmd = new SqlCommand("procGMDelegatedTask"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(connectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Ids", ids).SqlDbType = SqlDbType.Structured;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new LookupIntDTO { Value = reader.GetInt32(0) });
                    }
                }
                cmd.Connection?.Close();
            }
            return result;
        }

        public async Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId)
        {
            var list = await _repository.DelegationEmailDTO.FromSqlInterpolated($"exec procGMDelegatedTask @action = 3, @delegationid = {delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<GMDueDate> updated)
        {
            var ids = new DataTable();
            ids.Columns.Add("Id", typeof(Int32));
            ids.Columns.Add("DueDate", typeof(DateTime));

            var result = new List<LookupIntDTO>();
            foreach (var item in updated)
            {
                var newRec = ids.NewRow();
                newRec["Id"] = item.DDId;
                newRec["DueDate"] = item.DueDate;
                ids.Rows.Add(newRec);
            }
            
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;
            using (SqlCommand cmd = new SqlCommand("procGMDelegatedTask"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(connectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Ids", ids).SqlDbType = SqlDbType.Structured;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new LookupIntDTO { Value = reader.GetInt32(0) });
                    }
                }
                cmd.Connection?.Close();
            }
            return result;
        }

        public async Task<List<GMWorkflowActionParameter>> CheckWorkflowActionParameters(GMWorkflowTriggerType triggerType)
        {
            var actionParameters = await _repository.GMWorkflowActionParameters.Where(w => w.Workflow.TriggerTypeId == (int)triggerType && w.Workflow.ActiveSwitch).Include(w => w.Workflow).ToListAsync();
            return actionParameters;
        }

        public async Task<bool> HasWorkflowEnabled(GMWorkflowTriggerType triggerType)
        {
            return await _repository.GMWorkflows.AnyAsync(wf => wf.TriggerTypeId == (int)triggerType && wf.ActiveSwitch);
        }


        public async Task<List<GMDueDate>> GenerateDueDateFromActionParameterWorkflow(GMActionDue? newActionDue, List<GMDueDate> dueDates, GMWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActionParameters = await CheckWorkflowActionParameters(triggerType);
            if (!workflowActionParameters.Any())
                return null;

            var dueDateindicators = dueDates.Select(d => d.Indicator).ToList();
            var indicators = await _repository.GMIndicators.ToListAsync();
            workflowActionParameters = workflowActionParameters.Where(w => indicators.Any(i => i.IndicatorId == w.Workflow.TriggerValueId && dueDateindicators.Any(s => s.ToLower() == i.Indicator.ToLower()))).ToList();

            if (!workflowActionParameters.Any())
                return null;

            var gm = new GMMatter();

            //from duedate grid insert
            if (newActionDue.ActId > 0 && string.IsNullOrEmpty(newActionDue.CaseNumber))
            {
                newActionDue = await _repository.GMActionsDue.Where(ad => ad.ActId == newActionDue.ActId).Include(ad => ad.GMMatter).AsNoTracking().FirstOrDefaultAsync();
                if (newActionDue != null)
                    gm = newActionDue.GMMatter;
                else
                    gm = null;
            }
            else if (!string.IsNullOrEmpty(newActionDue.CaseNumber))
            {
                gm = await _repository.GMMatters.Where(ca => ca.CaseNumber == newActionDue.CaseNumber && ca.SubCase == newActionDue.SubCase).AsNoTracking().FirstOrDefaultAsync();
            }

            if (gm != null)
            {
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + gm.ClientID.ToString() + "|"))).ToList();
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.MatterTypeFilter) || (w.Workflow.MatterTypeFilter != null && w.Workflow.MatterTypeFilter.Contains("|" + gm.MatterType + "|"))).ToList();
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + gm.RespOffice + "|"))).ToList();

                if (workflowActionParameters.Any(w => !string.IsNullOrEmpty(w.Workflow.AttorneyFilter)))
                {
                    var matterAttorneys = await _repository.GMMatterAttorneys.Where(a => a.MatId == gm.MatId).ToListAsync();
                    workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null && matterAttorneys.Any(a => w.Workflow.AttorneyFilter.Contains("|" + a.AttorneyID.ToString() + "|")))).ToList();
                }
                
                if (clearBase)
                {
                    workflowActionParameters = ClearGMBaseWorkflowActionParameters(workflowActionParameters);
                }

                if (!workflowActionParameters.Any())
                    return null;

                var newDueDates = new List<GMDueDate>();
                var basedOns = dueDates.Where(dd => indicators.Any(i => i.Indicator.ToLower() == dd.Indicator.ToLower() && workflowActionParameters.Any(wf => wf.Workflow.TriggerValueId == i.IndicatorId))).ToList();

                foreach (var dd in basedOns)
                {
                    foreach (var item in workflowActionParameters.Where(w => indicators.Any(i => i.IndicatorId == w.Workflow.TriggerValueId && i.Indicator.ToLower() == dd.Indicator.ToLower())).ToList())
                    {
                        //based on DueDate 
                        var computedDueDate = dd.DueDate.AddYears(item.Yr).AddMonths(item.Mo).AddDays((double)item.Dy);

                        //proper leap year handling
                        //var computedDueDate = actionDue.BaseDate.AddYears(ap.Yr).AddMonths(ap.Mo).AddDays((double)ap.Dy),

                        //make sure it is non existing
                        if (!dueDates.Any(edd => edd.ActionDue.ToLower() == item.ActionDue.ToLower() && edd.DueDate == computedDueDate) &&
                            !newDueDates.Any(ndd => ndd.ActionDue.ToLower() == item.ActionDue.ToLower() && ndd.DueDate == computedDueDate))
                            {
                            var dueDate = new GMDueDate()
                            {
                                ActId = 0,
                                ActionDue = item.ActionDue,
                                DueDate = computedDueDate,
                                DateTaken = newActionDue.ResponseDate,
                                Indicator = item.Indicator,
                                AttorneyID = newActionDue.ResponsibleID,
                                CreatedBy = newActionDue.UpdatedBy,
                                DateCreated = newActionDue.LastUpdate,
                                UpdatedBy = newActionDue.UpdatedBy,
                                LastUpdate = newActionDue.LastUpdate
                            };
                            newDueDates.Add(dueDate);
                        }
                    }
                }
                return newDueDates;
            }
            return null;
        }

        public async Task<List<GMDueDate>> GetUpdatedDueDateIndicator(int actId, List<GMDueDate> dueDates)
        {
            var existingDueDates = await _repository.GMDueDates.Where(dd => dd.ActId == actId).AsNoTracking().ToListAsync();
            if (existingDueDates.Any())
            {

                var updatedDueDates = dueDates.Where(udd => existingDueDates.Any(dd => udd.DDId == dd.DDId && udd.Indicator.ToLower() != dd.Indicator.ToLower())).ToList();
                return updatedDueDates;
            }
            return null;
        }


        private List<GMWorkflowActionParameter> ClearGMBaseWorkflowActionParameters(List<GMWorkflowActionParameter> workflowActions)
        {

            //with filter will override the record with no filter at all
            foreach (var item in workflowActions.Where(wf => !(string.IsNullOrEmpty(wf.Workflow.ClientFilter) && string.IsNullOrEmpty(wf.Workflow.MatterTypeFilter)
                                                              && string.IsNullOrEmpty(wf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(wf.Workflow.AttorneyFilter))).ToList())
            {
                workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && string.IsNullOrEmpty(bf.Workflow.MatterTypeFilter)
                                                              && string.IsNullOrEmpty(bf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(bf.Workflow.AttorneyFilter) && bf.ActionDue == item.ActionDue && bf.Workflow.TriggerValueId == item.Workflow.TriggerValueId && (bf.Workflow.TriggerValueName ?? "") == (item.Workflow.TriggerValueName ?? ""));
            }
            return workflowActions;
        }




        #endregion
        public async Task UpdateDeDocket(GMMatter matter)
        {
            await ValidatePermission(matter.MatId, CPiPermissions.DeDocketer);
            await ValidateMatter(matter, CPiPermissions.DeDocketer);

            var deDocketFields = await _systemSettingManager.GetSystemSetting<DeDocketFields>();
            var updated = await GetByIdAsync(matter.MatId);

            Guard.Against.NoRecordPermission(updated != null);

            if (updated != null && deDocketFields.GeneralMatter != null)
            {
                if (deDocketFields.GeneralMatter.MatterTitle)
                    updated.MatterTitle = matter.MatterTitle;

                if (deDocketFields.GeneralMatter.ClientReference)
                    updated.ClientRef = matter.ClientRef;

                if (deDocketFields.GeneralMatter.OtherReferenceNumber)
                    updated.OtherReferenceNumber = matter.OtherReferenceNumber;

                if (deDocketFields.GeneralMatter.AgentReference)
                    updated.ReferenceNumber = matter.ReferenceNumber;

                if (deDocketFields.GeneralMatter.Remarks)
                    updated.Remarks = matter.Remarks;

                updated.LastUpdate = matter.LastUpdate;
                updated.UpdatedBy = matter.UpdatedBy;
                updated.tStamp = matter.tStamp;

                _cpiDbContext.GetRepository<GMMatter>().Update(updated);
                await _cpiDbContext.SaveChangesAsync();

            }
            else
                Guard.Against.UnAuthorizedAccess(false);
        }
        public IQueryable<DeDocketInstruction> DeDocketInstructions
        {
            get
            {
                return _repository.DeDocketInstructions.AsNoTracking();
            }
        }

        public void DetachAllEntities()
        {
            _repository.DetachAllEntities();
        }
        public List<EntityEntry> GetAllTrackedEntities()
        {
            return _repository.GetAllTrackedEntities();
        }

        public async Task<int> GetRequestDocketPendingCount(int matId)
        {
            return await _repository.GMDocketRequests.Where(r => r.MatId == matId && r.CompletedDate == null).CountAsync();
        }

        public async Task<List<GMDocketRequest>> GetRequestDockets(int matId, bool outstandingOnly)
        {
            return await _repository.GMDocketRequests.Where(r => r.MatId == matId && (!outstandingOnly || (outstandingOnly && r.CompletedDate == null))).ToListAsync();
        }


    }
}
