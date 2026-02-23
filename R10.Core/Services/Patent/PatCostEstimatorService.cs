using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace R10.Core.Services
{
    public class PatCostEstimatorService : EntityService<PatCostEstimator>, IPatCostEstimatorService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<PatSetting> _settings;

        public PatCostEstimatorService(
            ICPiDbContext cpiDbContext,
            IApplicationDbContext repository,
            ClaimsPrincipal user,
            ISystemSettings<PatSetting> settings) : base(cpiDbContext, user)
        {
            _repository = repository;
            _settings = settings;
        }

        public IQueryable<PatCostEstimator> PatCostEstimators
        {
            get
            {
                IQueryable<PatCostEstimator> costEstimators = base.QueryableList;

                if (!_user.IsAdmin())
                {
                    costEstimators = costEstimators.Where(d => d.UserId == _user.GetUserIdentifier());
                }

                return costEstimators;
            }
        }
        public IQueryable<PatCostEstimatorBaseAppDTO> PatCostEstimatorBaseApps => _repository.PatCostEstimatorBaseAppDTO.AsNoTracking();

        public override async Task<PatCostEstimator> GetByIdAsync(int keyId)
        {
            return await QueryableList.SingleOrDefaultAsync(ce => ce.KeyId == keyId);
        }

        public override async Task Add(PatCostEstimator costEstimator)
        {
            Guard.Against.NoRecordPermission(_user.IsInRoles(SystemType.Patent, CPiPermissions.CostEstimatorModify));
            if (string.IsNullOrEmpty(costEstimator.UserId)) costEstimator.UserId = _user.GetUserIdentifier();
            await base.Add(costEstimator);
        }

        public override async Task Update(PatCostEstimator costEstimator)
        {
            await ValidatePermission(costEstimator.KeyId, CPiPermissions.CostEstimatorModify);

            var oldCostEstimator = await PatCostEstimators.Where(d => d.KeyId == costEstimator.KeyId).FirstOrDefaultAsync();

            //Clear added Countries and Questions if Applicant changes
            if (oldCostEstimator != null && oldCostEstimator.Applicant != costEstimator.Applicant)
            {
                //Delete countries questions
                var questions = await _repository.PatCostEstimatorCountryCosts.Where(d => d.KeyId == costEstimator.KeyId).ToListAsync();
                var countries = await _repository.PatCostEstimatorCountries.Where(d => d.KeyId == costEstimator.KeyId).ToListAsync();
                _cpiDbContext.GetRepository<PatCostEstimatorCountryCost>().Delete(questions);
                _cpiDbContext.GetRepository<PatCostEstimatorCountry>().Delete(countries);
                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(questions);
                _cpiDbContext.Detach(countries);
            }

            await base.Update(costEstimator);
            _cpiDbContext.Detach(costEstimator);
        }

        public override async Task UpdateRemarks(PatCostEstimator entity)
        {
            await ValidatePermission(entity.KeyId, CPiPermissions.CostEstimatorRemarksOnly);
            await base.UpdateRemarks(entity);
        }

        public override async Task Delete(PatCostEstimator costEstimator)
        {
            await ValidatePermission(costEstimator.KeyId, CPiPermissions.CostEstimatorModify);
            await base.Delete(costEstimator);
        }

        public async Task ValidatePermission(int keyId, List<string> roles)
        {
            var recordKeyId = await PatCostEstimators.Where(d => d.KeyId == keyId).Select(d => d.KeyId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(recordKeyId > 0);
            Guard.Against.NoRecordPermission(_user.IsInRoles(SystemType.Patent, roles));
        }

        public async Task CopyCostEstimator(int oldKeyId, int newKeyId, string userName, bool copyCountries, bool copyAnswers)
        {
            await _cpiDbContext.ExecuteSqlRawAsync($"procPatCostEstimatorCopy @OldKeyId={oldKeyId},@NewKeyId={newKeyId},@CreatedBy='{userName}',@CopyCountries={copyCountries},@CopyAnswers={copyAnswers}");
        }

        public async Task AddCountryCosts(int keyId, string userName, List<int> addedCECountryIds)
        {
            // 1. Prepare and add main costs to PatCostEstimatorCost
            var costsToDuplicate = await _cpiDbContext.GetRepository<PatCECountryCost>().QueryableList
                .AsNoTracking()
                .Where(d => d.ActiveSwitch == true && addedCECountryIds.Contains(d.CECountryId))
                .OrderBy(o => o.PatCEStage!.StageOrder).ThenBy(t => t.OrderOfEntry)
                .ToListAsync();

            var newCECosts = costsToDuplicate
                .Select((d, index) => new PatCostEstimatorCost
                {
                    KeyId = keyId,
                    CostId = d.CostId,
                    CECountryId = d.CECountryId,
                    Description = d.Description,
                    DataType = d.DataType,
                    DefaultValue = d.DefaultValue,
                    Cost = d.Cost,
                    AltCost = d.AltCost,
                    Opts = d.Opts,
                    OrderOfEntry = index + 1,
                    MultCost = d.MultCost,
                    CostType = d.CostType,
                    Stage = d.Stage,
                    UseCostFactor = d.UseCostFactor,
                    CostFormula = d.CostFormula,
                    CostFactor1 = d.CostFactor1,
                    CostFactor2 = d.CostFactor2,
                    CostFactor3 = d.CostFactor3,
                    TranslationType = d.TranslationType,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                }).ToList();

            if (!newCECosts.Any()) return;

            _cpiDbContext.GetRepository<PatCostEstimatorCost>().Add(newCECosts);
            await _cpiDbContext.SaveChangesAsync();

            // 2. Prepare PatCostEstimatorCountryCost entries for main costs
            var newCECountryCosts = newCECosts
                .Select(d => new PatCostEstimatorCountryCost
                {
                    KeyId = keyId,
                    CECostId = d.CECostId,
                    AnswerStatus = CEAnswerStatus.NotSet,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                }).ToList();

            var newCostIds = newCECosts.Select(d => d.CostId).Distinct().ToList();

            // 3. Prepare and add child costs to PatCostEstimatorCostChild
            var childsToDuplicate = await _cpiDbContext.GetRepository<PatCECountryCostChild>().QueryableList
                .AsNoTracking()
                .Where(d => d.CActiveSwitch == true && newCostIds.Contains(d.CostId))
                .Include(d => d.PatCurrencyType)
                .ToListAsync();

            var newCEChilds = childsToDuplicate.Select(d => new PatCostEstimatorCostChild
            {
                CECostId = newCECosts.First(c => c.CostId == d.CostId).CECostId,
                CCId = d.CCId,
                CostId = d.CostId,
                KeyId = keyId,
                CDescription = d.CDescription,
                CDataType = d.CDataType,
                CDefaultValue = d.CDefaultValue,
                CAltValue = d.CAltValue,
                CCost = d.CCost,
                CAltCost = d.CAltCost,
                CMultCost = d.CMultCost,
                COpts = d.COpts,
                CAltOpts = d.CAltOpts,
                COrderOfEntry = d.COrderOfEntry,
                CurrencyType = d.CurrencyType,
                ExchangeRate = d.PatCurrencyType != null ? d.PatCurrencyType.ExchangeRate : null,
                AllowanceRate = d.PatCurrencyType != null ? d.PatCurrencyType.AllowanceRate : null,
                CreatedBy = userName,
                UpdatedBy = userName,
                DateCreated = DateTime.Now,
                LastUpdate = DateTime.Now
            }).ToList();

            if (newCEChilds.Any())
            {
                _cpiDbContext.GetRepository<PatCostEstimatorCostChild>().Add(newCEChilds);
                await _cpiDbContext.SaveChangesAsync();

                // 4. Prepare PatCostEstimatorCountryCost entries for child costs
                newCECountryCosts.AddRange(newCEChilds
                    .Where(d => d.CDataType == CECostDataType.Numeric || d.CDataType == CECostDataType.Boolean)
                    .Select(d => new PatCostEstimatorCountryCost
                    {
                        KeyId = keyId,
                        CECostId = d.CECostId,
                        CECCId = d.CECCId,
                        AnswerStatus = CEAnswerStatus.NotSet,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    }));

                var newCCIds = newCEChilds.Select(d => d.CCId).ToList();

                // 5. Prepare and add sub costs to PatCostEstimatorCostSub
                var subsToDuplicate = await _cpiDbContext.GetRepository<PatCECountryCostSub>().QueryableList
                    .AsNoTracking()
                    .Where(d => d.SActiveSwitch == true && newCCIds.Contains(d.CCId))
                    .ToListAsync();

                var newCESubs = subsToDuplicate.Select(d => new PatCostEstimatorCostSub
                {                    
                    CECostId = newCEChilds.First(c => c.CCId == d.CCId).CECostId,
                    CECCId = newCEChilds.First(c => c.CCId == d.CCId).CECCId,
                    SubId = d.SubId,
                    CCId = d.CCId,
                    KeyId = keyId,
                    SDescription = d.SDescription,
                    SDataType = d.SDataType,
                    SDefaultValue = d.SDefaultValue,
                    SAltValue = d.SAltValue,
                    SCost = d.SCost,
                    SAltCost = d.SAltCost,
                    SMultCost = d.SMultCost,
                    SOpts = d.SOpts,
                    SAltOpts = d.SAltOpts,
                    SOrderOfEntry = d.SOrderOfEntry,
                    SUseCostFactor = d.SUseCostFactor,
                    SCostFormula = d.SCostFormula,
                    SCostFactor1 = d.SCostFactor1,
                    SCostFactor2 = d.SCostFactor2,
                    SCostFactor3 = d.SCostFactor3,
                    STranslationType = d.STranslationType,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                }).ToList();

                if (newCESubs.Any())
                {
                    _cpiDbContext.GetRepository<PatCostEstimatorCostSub>().Add(newCESubs);
                    await _cpiDbContext.SaveChangesAsync();

                    // 6. Prepare PatCostEstimatorCountryCost entries for sub costs
                    newCECountryCosts.AddRange(newCESubs.Select(d => new PatCostEstimatorCountryCost
                    {
                        KeyId = keyId,
                        CECostId = d.CECostId,
                        CECCId = d.CECCId,
                        CESubId = d.CESubId,
                        AnswerStatus = CEAnswerStatus.NotSet,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    }));
                }
            }

            // 7. Add all new PatCostEstimatorCountryCost entries
            if (newCECountryCosts.Any())
            {
                _cpiDbContext.GetRepository<PatCostEstimatorCountryCost>().Add(newCECountryCosts);
                await _cpiDbContext.SaveChangesAsync();
            }

            // 8. Handle answer cascading in a single, efficient query
            await CascadeAnswersAsync(keyId, newCECountryCosts);

            _cpiDbContext.DetachAll();
        }

        private async Task CascadeAnswersAsync(int keyId, List<PatCostEstimatorCountryCost> newAddedCosts)
        {
            var hasChanges = false;
    
            // Get all relevant cascade groups for the new costs in a single query
            var allNewCostIds = newAddedCosts.Where(d => d.CECostId > 0 && (d.CECCId == null || d.CECCId <= 0) && (d.CESubId == null || d.CESubId <= 0)).Select(d => d.CECostId).ToList();
            var allNewChildIds = newAddedCosts.Where(d => d.CECCId > 0).Select(d => d.CECCId ?? 0).ToList();
            var allNewSubIds = newAddedCosts.Where(d => d.CESubId > 0).Select(d => d.CESubId ?? 0).ToList();

            var allCascadeGroups = await GetCascadeCosts(allNewCostIds, allNewChildIds, allNewSubIds);

            var cascadeLookup = allCascadeGroups.ToDictionary(
                k => (k.DataKey!.ToLower(), k.DataKeyValue),
                v => v.GroupId
            );

            var originalAnswers = await _cpiDbContext.GetRepository<PatCostEstimatorCountryCost>().QueryableList
                .AsNoTracking()
                .Where(d => d.KeyId == keyId && !string.IsNullOrEmpty(d.Answer) && d.AnswerStatus == CEAnswerStatus.Original)
                .OrderBy(o => o.PatCostEstimatorCost!.PatCECountrySetup!.Country)
                .ToListAsync();

            var cascadedAnswers = await _cpiDbContext.GetRepository<PatCostEstimatorCountryCost>().QueryableList
                .AsNoTracking()
                .Where(d => d.KeyId == keyId && !string.IsNullOrEmpty(d.Answer) && d.AnswerStatus == CEAnswerStatus.Cascaded)
                .OrderBy(o => o.PatCostEstimatorCost!.PatCECountrySetup!.Country)
                .ToListAsync();

            var originalAnswerLookup = new Dictionary<int, string>();
            foreach(var answerEntry in originalAnswers)
            {
                if (TryGetGroupIdForAnswer(answerEntry, cascadeLookup, out int groupId))
                {
                    if (!originalAnswerLookup.ContainsKey(groupId))
                    {
                        originalAnswerLookup.Add(groupId, answerEntry.Answer ?? "");
                    }
                }
            }

            var cascadedAnswerLookup = new Dictionary<int, string>();
            foreach(var answerEntry in cascadedAnswers)
            {
                if (TryGetGroupIdForAnswer(answerEntry, cascadeLookup, out int groupId))
                {
                    if (!cascadedAnswerLookup.ContainsKey(groupId))
                    {
                        cascadedAnswerLookup.Add(groupId, answerEntry.Answer ?? "");
                    }
                }
            }

            foreach (var ceCost in newAddedCosts)
            {
                string answerToCascade = string.Empty;
                int? determinedGroupId = null;

                // Determine the DataKey and DataKeyValue for the current ceCost
                string dataKey = null;
                int dataValue = 0;

                if (ceCost.CECostId > 0 && (ceCost.CECCId == null || ceCost.CECCId <= 0) && (ceCost.CESubId == null || ceCost.CESubId <= 0))
                {
                    dataKey = "cecostid";
                    dataValue = ceCost.CECostId;
                }
                else if (ceCost.CECostId > 0 && ceCost.CECCId > 0 && (ceCost.CESubId == null || ceCost.CESubId <= 0))
                {
                    dataKey = "ceccid";
                    dataValue = ceCost.CECCId ?? 0;
                }
                else if (ceCost.CECostId > 0 && ceCost.CECCId > 0 && ceCost.CESubId > 0)
                {
                    dataKey = "cesubid";
                    dataValue = ceCost.CESubId ?? 0;
                }

                // Try to get the GroupId using the lookup dictionary
                if (dataKey != null && cascadeLookup.TryGetValue((dataKey, dataValue), out int groupId))
                {
                    determinedGroupId = groupId;

                    // Try to find an answer from original answers first
                    if (originalAnswerLookup.TryGetValue(groupId, out string originalAnswer))
                    {
                        answerToCascade = originalAnswer;
                    }
                    // If not found in original, try from cascaded answers
                    else if (cascadedAnswerLookup.TryGetValue(groupId, out string cascadedAnswer))
                    {
                        answerToCascade = cascadedAnswer;
                    }
                }

                if (!string.IsNullOrEmpty(answerToCascade))
                {
                    hasChanges = true;
                    ceCost.Answer = answerToCascade;
                    ceCost.AnswerStatus = CEAnswerStatus.Cascaded;
                }
            }

            if (hasChanges)
            {
                await _repository.SaveChangesAsync();
            }
        }

        private bool TryGetGroupIdForAnswer(PatCostEstimatorCountryCost answerEntry, Dictionary<(string DataKey, int DataKeyValue), int> cascadeLookup, out int groupId)
        {
            groupId = 0;
            string dataKey = string.Empty;
            int dataValue = 0;

            if (answerEntry.CECostId > 0 && (answerEntry.CECCId == null || answerEntry.CECCId <= 0) && (answerEntry.CESubId == null || answerEntry.CESubId <= 0))
            {
                dataKey = "cecostid";
                dataValue = answerEntry.CECostId;
            }
            else if (answerEntry.CECostId > 0 && answerEntry.CECCId > 0 && (answerEntry.CESubId == null || answerEntry.CESubId <= 0))
            {
                dataKey = "ceccid";
                dataValue = answerEntry.CECCId ?? 0;
            }
            else if (answerEntry.CECostId > 0 && answerEntry.CECCId > 0 && answerEntry.CESubId > 0)
            {
                dataKey = "cesubid";
                dataValue = answerEntry.CESubId ?? 0;
            }

            if (dataKey != null && cascadeLookup.TryGetValue((dataKey, dataValue), out int foundGroupId))
            {
                groupId = foundGroupId;
                return true;
            }
            return false;
        }

        public async Task DeleteCountryCosts(int keyId, string userName, List<int> deletedEntityIds)
        {
            //Remove country questions based on country code - don't care about casetype/entitystatus
            await ValidatePermission(keyId, CPiPermissions.CostEstimatorModify);

            var oldCECountryIds = _repository.PatCostEstimatorCountries.AsNoTracking()
                                    .AsEnumerable()
                                    .Where(d => deletedEntityIds.Contains(d.EntityId))
                                    .Select(d => d.CECountryId)
                                    .ToList();

            var oldCECosts = await _repository.PatCostEstimatorCountryCosts
                                    .Where(d => d.KeyId == keyId && d.PatCostEstimatorCost != null && oldCECountryIds.Contains(d.PatCostEstimatorCost.CECountryId))
                                    .ToListAsync();

            if (oldCECosts.Count > 0)
            {
                _cpiDbContext.GetRepository<PatCostEstimatorCountryCost>().Delete(oldCECosts);
                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(oldCECosts);
            }

            var oldCECostDetails = await _repository.PatCostEstimatorCosts
                                    .Where(d => d.KeyId == keyId && oldCECountryIds.Contains(d.CECountryId)).ToListAsync();

            if (oldCECostDetails.Count > 0)
            {
                _cpiDbContext.GetRepository<PatCostEstimatorCost>().Delete(oldCECostDetails);
                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(oldCECostDetails);
            }
        }

        public async Task<List<CEEstimatedCostDTO>> GetEstimatedCosts(int keyId)
        {
            try
            {
                var result = await _repository.CEEstimatedCostDTOs.FromSqlInterpolated($"procWebPatCostEstimatorGetAllCost @KeyId={keyId}").AsNoTracking().ToListAsync();
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<List<CECascadeCostDTO>> GetCascadeCosts(List<int> ceCostIds, List<int> ceCCIds, List<int> ceSubIds)
        {
            const int nonExistentId = -1;
            var costIdParams = (ceCostIds != null && ceCostIds.Count > 0) ? string.Join(", ", ceCostIds.Select((_, i) => $"@costId_{i}")) : nonExistentId.ToString();
            var childIdParams = (ceCCIds != null && ceCCIds.Count > 0) ? string.Join(", ", ceCCIds.Select((_, i) => $"@childId_{i}")) : nonExistentId.ToString();
            var subIdParams = (ceSubIds != null && ceSubIds.Count > 0) ? string.Join(", ", ceSubIds.Select((_, i) => $"@subId_{i}")) : nonExistentId.ToString();

            var sql = $@"
                SELECT g.GroupId, g.DataKey, g.DataKeyValue
                FROM tblPatCostEstimatorCostGuide AS g
                WHERE g.GroupId IN (
                    SELECT DISTINCT tmp.GroupId
                    FROM tblPatCostEstimatorCostGuide AS tmp
                    WHERE
                        (tmp.DataKey = 'CECostId' AND tmp.DataKeyValue IN ({costIdParams})) OR
                        (tmp.DataKey = 'CECCId' AND tmp.DataKeyValue IN ({childIdParams})) OR
                        (tmp.DataKey = 'CESubId' AND tmp.DataKeyValue IN ({subIdParams}))
                )";

            var parameters = new List<SqlParameter>();

            if (ceCostIds != null && ceCostIds.Count > 0)
            {
                parameters.AddRange(ceCostIds.Select((id, i) => new SqlParameter($"@costId_{i}", id)));
            }
            if (ceCCIds != null && ceCCIds.Count > 0)
            {
                parameters.AddRange(ceCCIds.Select((id, i) => new SqlParameter($"@childId_{i}", id)));
            }
            if (ceSubIds != null && ceSubIds.Count > 0)
            {
                parameters.AddRange(ceSubIds.Select((id, i) => new SqlParameter($"@subId_{i}", id)));
            }

            // Execute the query
            var allCascadeGroups = await _repository.CECascadeCostDTOs
                .FromSqlRaw(sql, parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            return allCascadeGroups;
        }

        /// <summary>
        /// Cascades answers from updated costs to other related costs within the same cascade group
        /// that currently have no answer.
        /// </summary>
        /// <param name="keyId">The ID of the main cost estimator record.</param>
        /// <param name="updatedList">A list of costs that have been updated by the user.</param>
        public async Task CascadeAnswersByGridAsync(int keyId, List<PatCostEstimatorCountryCost> updatedList)
        {
            /* Cascade answer to other costs with same wording and data type.
            *  Only cascade answer to costs without answer
            *  Set AnswerStatus for current updating cost to Original if AnswerStatus is NotSet and there are no cascading costs that have Answer or AnswerStatus is Original in the same group
            */

            // 1. Collect all unique IDs from the updated costs
            // This allows us to fetch all relevant cascade group data in a single database query.
            var currentUpdatingCECostIds = updatedList.Where(d => d.CECostId > 0 && (d.CECCId == null || d.CECCId <= 0) && (d.CESubId == null || d.CESubId <= 0))
                .Select(d => d.CECostId).ToList();
            var currentUpdatingCECCIds = updatedList.Where(d => d.CECostId > 0 && d.CECCId > 0 && (d.CESubId == null || d.CESubId <= 0))
                .Select(d => d.CECCId ?? 0).ToList();
            var currentUpdatingCESubIds = updatedList.Where(d => d.CECostId > 0 && d.CECCId > 0 && d.CESubId > 0)
                .Select(d => d.CESubId ?? 0).ToList();

            // 2. Fetch all cascade group mappings for the updated costs
            var allCascadeCosts = await GetCascadeCosts(currentUpdatingCECostIds, currentUpdatingCECCIds, currentUpdatingCESubIds);

            // 3. Create a dictionary for efficient GroupId lookup
            var groupLookup = allCascadeCosts.ToDictionary(k => (k.DataKey!.ToLower(), k.DataKeyValue), v => v.GroupId);

            // 4. Fetch all existing costs for the current record, excluding the ones in the updated list
            var updatedEntityIds = updatedList.Select(d => d.EntityId).ToList();
            var allExistingCosts = await _cpiDbContext.GetRepository<PatCostEstimatorCountryCost>().QueryableList
                .Where(d => d.KeyId == keyId && !updatedEntityIds.Contains(d.EntityId)).ToListAsync();

            // 5. Loop through each cost that was updated by the user and has an answer
            var updatedListCheckCascade = updatedList.Where(d => !string.IsNullOrEmpty(d.Answer)).ToList();
            foreach (var updatedCost in updatedListCheckCascade)
            {
                // a. Determine the DataKey and DataValue for the updated cost
                string dataKey = string.Empty;
                int dataValue = 0;
                if (updatedCost.CECostId > 0 && (updatedCost.CECCId == null || updatedCost.CECCId <= 0) && (updatedCost.CESubId == null || updatedCost.CESubId <= 0))
                {
                    dataKey = "cecostid";
                    dataValue = updatedCost.CECostId;
                }
                else if (updatedCost.CECostId > 0 && updatedCost.CECCId > 0 && (updatedCost.CESubId == null || updatedCost.CESubId <= 0))
                {
                    dataKey = "ceccid";
                    dataValue = updatedCost.CECCId ?? 0;
                }
                else if (updatedCost.CECostId > 0 && updatedCost.CECCId > 0 && updatedCost.CESubId > 0)
                {
                    dataKey = "cesubid";
                    dataValue = updatedCost.CESubId ?? 0;
                }

                // b. Find the GroupId for the current updated cost using the efficient dictionary lookup
                if (!string.IsNullOrEmpty(dataKey) && groupLookup.TryGetValue((dataKey, dataValue), out int groupId))
                {
                    // c. Find all costs from the existing record that belong to the same cascade group
                    var relatedCostsInCascadeGroup = allExistingCosts.Where(d =>
                    {
                        string relatedDataKey = string.Empty;
                        int relatedDataValue = 0;
                        if (d.CECostId > 0 && (d.CECCId == null || d.CECCId <= 0) && (d.CESubId == null || d.CESubId <= 0))
                        {
                            relatedDataKey = "cecostid";
                            relatedDataValue = d.CECostId;
                        }
                        else if (d.CECostId > 0 && d.CECCId > 0 && (d.CESubId == null || d.CESubId <= 0))
                        {
                            relatedDataKey = "ceccid";
                            relatedDataValue = d.CECCId ?? 0;
                        }
                        else if (d.CECostId > 0 && d.CECCId > 0 && d.CESubId > 0)
                        {
                            relatedDataKey = "cesubid";
                            relatedDataValue = d.CESubId ?? 0;
                        }
                
                        return !string.IsNullOrEmpty(relatedDataKey) && groupLookup.TryGetValue((relatedDataKey, relatedDataValue), out int relatedGroupId) && relatedGroupId == groupId;
                    }).ToList();

                    // d. Find all costs from the updating record that belong to the same cascade group
                    var relatedCostsInUpdatedList = updatedList.Where(d => {
                        string relatedDataKey = string.Empty;
                        int relatedDataValue = 0;
                        if (d.CECostId > 0 && (d.CECCId == null || d.CECCId <= 0) && (d.CESubId == null || d.CESubId <= 0))
                        {
                            relatedDataKey = "cecostid";
                            relatedDataValue = d.CECostId;
                        }
                        else if (d.CECostId > 0 && d.CECCId > 0 && (d.CESubId == null || d.CESubId <= 0))
                        {
                            relatedDataKey = "ceccid";
                            relatedDataValue = d.CECCId ?? 0;
                        }
                        else if (d.CECostId > 0 && d.CECCId > 0 && d.CESubId > 0)
                        {
                            relatedDataKey = "cesubid";
                            relatedDataValue = d.CESubId ?? 0;
                        }
            
                        return !string.IsNullOrEmpty(relatedDataKey) && groupLookup.TryGetValue((relatedDataKey, relatedDataValue), out int relatedGroupId) && relatedGroupId == groupId;
                    }).ToList();

                    // e. Check if any other cost in the group already has an answer or an Original status
                    var hasOtherCostsWithAnswers = relatedCostsInCascadeGroup.Any(d =>
                        // Exclude the current updated cost
                        !(d.CECostId == updatedCost.CECostId && d.CECCId == updatedCost.CECCId && d.CESubId == updatedCost.CESubId)
                        && (!string.IsNullOrEmpty(d.Answer) || d.AnswerStatus == CEAnswerStatus.Original));

                    var hasUpdatingCostsWithAnswers = relatedCostsInUpdatedList.Any(d =>
                        // Exclude the current updated cost
                        !(d.CECostId == updatedCost.CECostId && d.CECCId == updatedCost.CECCId && d.CESubId == updatedCost.CESubId)
                        && !string.IsNullOrEmpty(d.Answer));

                    // f. If the current cost is the first one with an answer in its group, set its status to Original
                    if (!hasOtherCostsWithAnswers && !hasUpdatingCostsWithAnswers && updatedCost.AnswerStatus == CEAnswerStatus.NotSet)
                    {
                        updatedCost.AnswerStatus = CEAnswerStatus.Original;
                    }

                    // g. Cascade the answer to all other related costs that don't have an answer
                    foreach (var relatedCost in relatedCostsInCascadeGroup.Where(c => string.IsNullOrEmpty(c.Answer)))
                    {
                        relatedCost.Answer = updatedCost.Answer;
                        relatedCost.AnswerStatus = CEAnswerStatus.Cascaded;
                        updatedList.Add(relatedCost);
                    }
                }
            }
        }
    }
}