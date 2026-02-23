using System;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.DMS;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using R10.Core.Entities.Patent;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Core.Exceptions;
using R10.Core.DTOs;
using System.Transactions;
using System.ComponentModel;

namespace R10.Core.Services
{
    public class DMSQuestionnaireService : IDMSQuestionnaireService
    {
        private readonly ClaimsPrincipal _user;
        private readonly IApplicationDbContext _repository;

        public DMSQuestionnaireService(IApplicationDbContext repository, ClaimsPrincipal user)
        {
            _user = user;
            _repository = repository;
        }

        public IQueryable<DMSQuestionGroup> DMSQuestionGroups => _repository.DMSQuestionGroups.AsNoTracking();
        public IQueryable<DMSQuestionGuide> DMSQuestionGuides => _repository.DMSQuestionGuides;
        public IQueryable<DMSQuestionGuideChild> DMSQuestionGuideChildren => _repository.DMSQuestionGuideChildren;
        public IQueryable<DMSQuestionGuideSub> DMSQuestionGuideSubs => _repository.DMSQuestionGuideSubs;
        public IQueryable<DMSQuestionGuideSubDtl> DMSQuestionGuideSubDtls => _repository.DMSQuestionGuideSubDtls;

        public async Task AddQuestionGroup(DMSQuestionGroup questionGroup)
        {
            _repository.DMSQuestionGroups.Add(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateQuestionGroup(DMSQuestionGroup questionGroup)
        {
            _repository.DMSQuestionGroups.Update(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteQuestionGroup(DMSQuestionGroup questionGroup)
        {
            _repository.DMSQuestionGroups.Remove(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task Copy(int oldGroupId, int newGroupId, string userName, bool copyQuestions)
        {
            //Copy Questions
            if (copyQuestions)
            {
               var newGuides = await _repository.DMSQuestionGuides.AsNoTracking()
                                    .Where(d => d.GroupId == oldGroupId)
                                    .Select(d => new DMSQuestionGuide()
                                    {
                                        QuestionId = d.QuestionId,
                                        GroupId = newGroupId,
                                        Question = d.Question,
                                        OrderOfEntry = d.OrderOfEntry,
                                        AnswerType = d.AnswerType,
                                        ActiveSwitch = d.ActiveSwitch,
                                        RequiredOnSubmission = d.RequiredOnSubmission,
                                        AddToFuture = d.AddToFuture,
                                        Placeholder = d.Placeholder,
                                        FollowUp = d.FollowUp,
                                        CreatedBy = userName,
                                        UpdatedBy = userName,
                                        DateCreated = DateTime.Now,
                                        LastUpdate = DateTime.Now
                                    }).ToListAsync();
                if (newGuides != null && newGuides.Count > 0)
                {                    
                    var oldQuestionIds = newGuides.Select(d => d.QuestionId).ToList();

                    // Get child questions/selection
                    var newChilds = await _repository.DMSQuestionGuideChildren.AsNoTracking()
                                                .Where(d => oldQuestionIds.Contains(d.QuestionId))
                                                .Select(d => new DMSQuestionGuideChild()
                                                {
                                                    ChildId = d.ChildId,
                                                    QuestionId = d.QuestionId,                                                    
                                                    Description = d.Description,
                                                    OrderOfEntry = d.OrderOfEntry,
                                                    CAnswerType = d.CAnswerType,
                                                    CActiveSwitch = d.CActiveSwitch,
                                                    CRequiredOnSubmission = d.CRequiredOnSubmission,
                                                    CAddToFuture = d.CAddToFuture,
                                                    CPlaceholder = d.CPlaceholder,
                                                    CFollowUp = d.CFollowUp,
                                                    CreatedBy = userName,
                                                    UpdatedBy = userName,
                                                    DateCreated = DateTime.Now,
                                                    LastUpdate = DateTime.Now
                                                }).ToListAsync();

                    // Get sub questions/selections
                    if (newChilds != null && newChilds.Count > 0) {
                        var oldChildIds = newChilds.Select(d => d.ChildId).ToList();

                        var newSubs = await _repository.DMSQuestionGuideSubs.AsNoTracking()
                            .Where(d => oldChildIds.Contains(d.ChildId))
                            .Select(d => new DMSQuestionGuideSub()
                            {
                                SubId = d.SubId,
                                ChildId = d.ChildId,
                                Description = d.Description,
                                OrderOfEntry = d.OrderOfEntry,
                                SAnswerType = d.SAnswerType,
                                SActiveSwitch = d.SActiveSwitch,
                                SRequiredOnSubmission = d.SRequiredOnSubmission,
                                SAddToFuture = d.SAddToFuture,
                                SPlaceholder = d.SPlaceholder,
                                CreatedBy = userName,
                                UpdatedBy = userName,
                                DateCreated = DateTime.Now,
                                LastUpdate = DateTime.Now
                            }).ToListAsync();

                        // Get sub detail selections
                        if (newSubs != null && newSubs.Count > 0)
                        {
                            var oldSubIds = newSubs.Select(d => d.SubId).ToList();

                            var newSubDtls = await _repository.DMSQuestionGuideSubDtls.AsNoTracking()
                                .Where(d => oldSubIds.Contains(d.SubId))
                                .Select(d => new DMSQuestionGuideSubDtl()
                                {
                                    SubId = d.SubId,
                                    Description = d.Description,
                                    OrderOfEntry = d.OrderOfEntry,                               
                                    CreatedBy = userName,
                                    UpdatedBy = userName,
                                    DateCreated = DateTime.Now,
                                    LastUpdate = DateTime.Now
                                }).ToListAsync();

                            if (newSubDtls != null && newSubDtls.Count > 0)
                            {
                                foreach (var sub in newSubs)
                                {
                                    var newSubSubDtls = newSubDtls.Where(d => d.SubId == sub.SubId).ToList();
                                    if (newSubSubDtls != null && newSubSubDtls.Count > 0)
                                    {
                                        newSubSubDtls.ForEach(d => { d.SubId = 0; });
                                        sub.DMSQuestionGuideSubDtls = newSubSubDtls;
                                    }
                                }
                            }

                            foreach (var child in newChilds)
                            {
                                var newChildSubs = newSubs.Where(d => d.ChildId == child.ChildId).ToList();
                                if (newChildSubs != null && newChildSubs.Count > 0)
                                {
                                    newChildSubs.ForEach(d => { d.SubId = 0; d.ChildId = 0; });
                                    child.DMSQuestionGuideSubs = newChildSubs;
                                }
                            }
                        }                        

                        foreach (var guide in newGuides)
                        {
                            var newGuideChilds = newChilds.Where(d => d.QuestionId == guide.QuestionId).ToList();
                            if (newGuideChilds != null && newGuideChilds.Count > 0)
                            {
                                newGuideChilds.ForEach(d => { d.ChildId = 0; d.QuestionId = 0; });
                                guide.DMSQuestionGuideChildren = newGuideChilds;
                            }
                        }
                    }

                    newGuides.ForEach(d => { d.QuestionId = 0; });
                    
                    _repository.DMSQuestionGuides.AddRange(newGuides);
                    await _repository.SaveChangesAsync();
                }
            }
        }

        public async Task GenerateNewQuestions(int questionId)
        {
            //List of dmsId to add new question - disclosure status with CanSubmit=true and dms record doesn't have this question yet
            var dmsIds = await _repository.Disclosures
                .Where(d => d.DMSDisclosureStatus != null && d.DMSDisclosureStatus.CanSubmit 
                    && (d.DMSQuestions == null || !d.DMSQuestions.Any() || !d.DMSQuestions.Any(q => q.QuestionId == questionId)))
                .Select(d => d.DMSId).ToListAsync();

            if (dmsIds.Count > 0)
            {                
                var newDMSQuestions = new List<DMSQuestion>();
                var userName = _user.GetUserName();
                var today = DateTime.Now;

                var questionGuide = await _repository.DMSQuestionGuides.AsNoTracking().FirstOrDefaultAsync(d => d.QuestionId == questionId);
                if (questionGuide == null) return;

                // 1. Main Guide question
                newDMSQuestions.AddRange(dmsIds.Select(d => new DMSQuestion
                {
                    DMSId = d,
                    QuestionId = questionGuide.QuestionId,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = today,
                    LastUpdate = today
                }).ToList());

                // 2. Child follow up questions if Guide question is Boolean and FollowUp is true
                if (!string.IsNullOrEmpty(questionGuide.AnswerType) && questionGuide.AnswerType.ToLower() == "bool" && questionGuide.FollowUp)
                {
                    var templateChildQuestions = await _repository.DMSQuestionGuideChildren.AsNoTracking()
                    .Where(d => d.QuestionId == questionGuide.QuestionId && d.CAddToFuture)
                    .Select(d => new DMSQuestion()
                    {
                        DMSId = 0,
                        QuestionId = d.QuestionId,
                        ChildId = d.ChildId,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today
                    }).ToListAsync();

                    if (templateChildQuestions != null && templateChildQuestions.Count > 0)
                        newDMSQuestions.AddRange(dmsIds.SelectMany(dmsId =>
                            templateChildQuestions.Select(d => new DMSQuestion
                            {
                                DMSId = dmsId,
                                QuestionId = d.QuestionId,
                                ChildId = d.ChildId,
                                CreatedBy = d.CreatedBy,
                                UpdatedBy = d.UpdatedBy,
                                DateCreated = d.DateCreated,
                                LastUpdate = d.LastUpdate
                            })
                        ).ToList());
                }
                // 3. Sub follow up question if Guide question is Selection with Answer(s) has FollowUp is true
                else if (!string.IsNullOrEmpty(questionGuide.AnswerType) && questionGuide.AnswerType.ToLower() == "selection")
                {                    
                    var templateSubQuestions = await _repository.DMSQuestionGuideSubs.AsNoTracking()
                        .Where(d => d.SAddToFuture 
                            && d.DMSQuestionGuideChild != null 
                            && d.DMSQuestionGuideChild.QuestionId == questionGuide.QuestionId
                            && d.DMSQuestionGuideChild.CFollowUp
                            && !string.IsNullOrEmpty(d.DMSQuestionGuideChild.CAnswerType) && d.DMSQuestionGuideChild.CAnswerType.ToLower() == "string")
                        .Select(d => new DMSQuestion()
                        {
                            DMSId = 0,
                            QuestionId = d.DMSQuestionGuideChild!.QuestionId,
                            ChildId = d.ChildId,
                            SubId = d.SubId,
                            CreatedBy = userName,
                            UpdatedBy = userName,
                            DateCreated = today,
                            LastUpdate = today
                        }).ToListAsync();

                    if (templateSubQuestions != null && templateSubQuestions.Count > 0)
                        newDMSQuestions.AddRange(dmsIds.SelectMany(dmsId =>
                            templateSubQuestions.Select(d => new DMSQuestion
                            {
                                DMSId = dmsId,
                                QuestionId = d.QuestionId,
                                ChildId = d.ChildId,
                                SubId = d.SubId,
                                CreatedBy = d.CreatedBy,
                                UpdatedBy = d.UpdatedBy,
                                DateCreated = d.DateCreated,
                                LastUpdate = d.LastUpdate
                            })
                        ).ToList());
                }

                
                if (newDMSQuestions != null && newDMSQuestions.Count > 0)
                {
                    _repository.Set<DMSQuestion>().AddRange(newDMSQuestions);
                    await _repository.SaveChangesAsync();
                }                    
            }
        }


        #region Question Guide
        public async Task UpdateChild<T>(int parentId, string userName, IEnumerable<DMSQuestionGuide> updated, IEnumerable<DMSQuestionGuide> added, IEnumerable<T> deleted) where T : BaseEntity
        {            
            if (updated.Any())
            {
                //Check and delete child questions if AnswerType change              
                var updatedQuestions = updated.Where(d => !string.IsNullOrEmpty(d.AnswerType)).Select(d => new { d.QuestionId, d.AnswerType }).ToList();               
                
                var updatedQuestionIds = updatedQuestions.Select(d => d.QuestionId).Distinct().ToList();
                var existingQuestions = await DMSQuestionGuides.AsNoTracking().Where(d => updatedQuestionIds.Contains(d.QuestionId)).ToDictionaryAsync(d => d.QuestionId, d => d.AnswerType);

                var toDeleteChild_QuestionIds = new List<int>();

                foreach (var updatedQuestion in updatedQuestions)
                {
                    if (existingQuestions.TryGetValue(updatedQuestion.QuestionId, out var existingAnswerType))
                    {
                        if (!string.IsNullOrEmpty(existingAnswerType) && existingAnswerType != updatedQuestion.AnswerType)
                            toDeleteChild_QuestionIds.Add(updatedQuestion.QuestionId);
                    }
                }

                var childQuestions = await DMSQuestionGuideChildren.Where(d => toDeleteChild_QuestionIds.Contains(d.QuestionId)).ToListAsync();
                if (childQuestions.Any())
                    _repository.Set<DMSQuestionGuideChild>().RemoveRange(childQuestions);

                _repository.Set<DMSQuestionGuide>().UpdateRange(updated);                
            }               

            if (added.Any())
            {                
                var startIndex = await GetQuestionNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<DMSQuestionGuide>().AddRange(added);
            }                

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();            
        }

        public async Task DeleteQuestionGuide(int parentId, string userName, IEnumerable<DMSQuestionGuide> deleted)
        {
            if (deleted.Any())
            {
                await UpdateChild(parentId, userName, new List<DMSQuestionGuide>(), new List<DMSQuestionGuide>(), deleted);
            }
        }

        public async Task ReorderQuestionGuide(int id, string userName, int newIndex)
        {
            var questionGuide = await DMSQuestionGuides.SingleOrDefaultAsync(a => a.QuestionId == id);
            Guard.Against.NoRecordPermission(questionGuide != null);
            questionGuide.UpdatedBy = userName;
            questionGuide.LastUpdate = DateTime.Now;

            int groupId = questionGuide.GroupId;
            int oldIndex = questionGuide.OrderOfEntry;

            var questionGroup = await DMSQuestionGroups.Where(w => w.GroupId == groupId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(questionGroup != null);
            questionGroup.UpdatedBy = questionGuide.UpdatedBy;
            questionGroup.LastUpdate = questionGuide.LastUpdate;

            List<DMSQuestionGuide> questionGuides = new List<DMSQuestionGuide>();
            if (oldIndex > newIndex)
            {
                questionGuides = await DMSQuestionGuides.Where(w => w.GroupId == groupId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                questionGuides.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                questionGuides = await DMSQuestionGuides.Where(w => w.GroupId == groupId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                questionGuides.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            questionGuide.OrderOfEntry = newIndex;
            questionGuides.Add(questionGuide);

            _repository.Set<DMSQuestionGuide>().UpdateRange(questionGuides);
            _repository.DMSQuestionGroups.Update(questionGroup);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetQuestionNextOrderOfEntry(int groupId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await DMSQuestionGuides.Where(ma => ma.GroupId == groupId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        protected async Task UpdateParentStampsAsync(int groupId, string userName)
        {
            var questionGroup = await _repository.DMSQuestionGroups.Where(w => w.GroupId == groupId).FirstOrDefaultAsync();
            //var questionGroup = new DMSQuestionGroup() { GroupId = groupId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            questionGroup.UpdatedBy = userName;
            questionGroup.LastUpdate = DateTime.Now;

            var entity = _repository.DMSQuestionGroups.Attach(questionGroup);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }

        #endregion

        #region Question Guide Child
        public async Task QuestionGuideChildUpdate(int parentId, string userName, IEnumerable<DMSQuestionGuideChild> updated, IEnumerable<DMSQuestionGuideChild> added, IEnumerable<DMSQuestionGuideChild> deleted)
        {
            if (updated.Any())
            {
                //Check and delete sub selections/questions if AnswerType change from Selection or String (Selection) with FollowUp                
                var updatedChildren = updated.Where(d => !string.IsNullOrEmpty(d.CAnswerType)).Select(d => new { d.ChildId, d.CAnswerType }).ToList();
                
                var updatedChildIds = updatedChildren.Select(d => d.ChildId).Distinct().ToList();
                var existingChildren = await DMSQuestionGuideChildren.AsNoTracking().Where(d => updatedChildIds.Contains(d.ChildId)).ToDictionaryAsync(d => d.ChildId, d => d.CAnswerType);

                var toDeleteSub_ChildIds = new List<int>();

                foreach (var updatedChild in updatedChildren)
                {
                    if (existingChildren.TryGetValue(updatedChild.ChildId, out var existingAnswerType))
                    {
                        if (!string.IsNullOrEmpty(existingAnswerType) && existingAnswerType != updatedChild.CAnswerType)
                            toDeleteSub_ChildIds.Add(updatedChild.ChildId);
                    }
                }

                var subQuestions = await DMSQuestionGuideSubs.Where(d => toDeleteSub_ChildIds.Contains(d.ChildId)).ToListAsync();
                if (subQuestions.Any())
                    _repository.Set<DMSQuestionGuideSub>().RemoveRange(subQuestions);

                _repository.Set<DMSQuestionGuideChild>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetQuestionGuideChildNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<DMSQuestionGuideChild>().AddRange(added);
            }

            if (deleted.Any())
            {
                //SQL issue with multiple cascade paths - cannot do on delete cascade with foreign key
                //Delete existing questions in tblDMSQuestion
                var deleteChildIds = deleted.Select(d => d.ChildId).Distinct().ToList();
                await _repository.DMSQuestions.Where(d => deleteChildIds.Contains(d.ChildId ?? 0)).ExecuteDeleteAsync();

                _repository.Set<DMSQuestionGuideChild>().RemoveRange(deleted);
            }               

            await UpdateChildParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task ReorderQuestionGuideChild(int id, string userName, int newIndex)
        {
            var questionGuideChild = await DMSQuestionGuideChildren.SingleOrDefaultAsync(a => a.ChildId == id);
            Guard.Against.NoRecordPermission(questionGuideChild != null);
            questionGuideChild.UpdatedBy = userName;
            questionGuideChild.LastUpdate = DateTime.Now;

            int questionId = questionGuideChild.QuestionId;
            int oldIndex = questionGuideChild.OrderOfEntry;

            var questionGuide = await DMSQuestionGuides.Where(w => w.QuestionId == questionId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(questionGuide != null);
            questionGuide.UpdatedBy = questionGuideChild.UpdatedBy;
            questionGuide.LastUpdate = questionGuideChild.LastUpdate;

            List<DMSQuestionGuideChild> questionGuideChildren = new List<DMSQuestionGuideChild>();
            if (oldIndex > newIndex)
            {
                questionGuideChildren = await DMSQuestionGuideChildren.Where(w => w.QuestionId == questionId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                questionGuideChildren.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                questionGuideChildren = await DMSQuestionGuideChildren.Where(w => w.QuestionId == questionId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                questionGuideChildren.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            questionGuideChild.OrderOfEntry = newIndex;
            questionGuideChildren.Add(questionGuideChild);

            _repository.Set<DMSQuestionGuideChild>().UpdateRange(questionGuideChildren);
            _repository.DMSQuestionGuides.Update(questionGuide);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetQuestionGuideChildNextOrderOfEntry(int questionId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await DMSQuestionGuideChildren.Where(ma => ma.QuestionId == questionId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        protected async Task UpdateChildParentStampsAsync(int questionId, string userName)
        {
            var questionGuide = await _repository.DMSQuestionGuides.Where(w => w.QuestionId == questionId).FirstOrDefaultAsync();

            if (questionGuide != null)
            {
                questionGuide.UpdatedBy = userName;
                questionGuide.LastUpdate = DateTime.Now;

                var entity = _repository.DMSQuestionGuides.Attach(questionGuide);
                entity.Property(c => c.UpdatedBy).IsModified = true;
                entity.Property(c => c.LastUpdate).IsModified = true;

                var questionGroup = await _repository.DMSQuestionGroups.Where(d => d.GroupId == questionGuide.GroupId).FirstOrDefaultAsync();
                if (questionGroup != null)
                {
                    questionGroup.UpdatedBy = userName;
                    questionGroup.LastUpdate = DateTime.Now;

                    var groupEntity = _repository.DMSQuestionGroups.Attach(questionGroup);
                    groupEntity.Property(c => c.UpdatedBy).IsModified = true;
                    groupEntity.Property(c => c.LastUpdate).IsModified = true;
                }
            }
        }

        #endregion

        #region Question Guide Sub - only for Selection-choices for child or FollowUp questions for Selection-choices for child
        public async Task QuestionGuideSubUpdate(int parentId, string userName, IEnumerable<DMSQuestionGuideSub> updated, IEnumerable<DMSQuestionGuideSub> added, IEnumerable<DMSQuestionGuideSub> deleted)
        {
            if (updated.Any())
            {
                //Check and delete sub detail selections if AnswerType change from Selection
                var subIds = updated.Where(d => !string.IsNullOrEmpty(d.SAnswerType) && d.SAnswerType.ToLower() != "selection" ).Select(d => d.SubId).ToList();

                var subDtlQuestions = await DMSQuestionGuideSubDtls.Where(d => subIds.Contains(d.SubId)).ToListAsync();
                if (subDtlQuestions.Any())
                    _repository.Set<DMSQuestionGuideSubDtl>().RemoveRange(subDtlQuestions);

                _repository.Set<DMSQuestionGuideSub>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetQuestionGuideSubNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<DMSQuestionGuideSub>().AddRange(added);
            }

            if (deleted.Any())
            {                
                _repository.Set<DMSQuestionGuideSub>().RemoveRange(deleted);
            }

            await UpdateSubParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task ReorderQuestionGuideSub(int id, string userName, int newIndex)
        {
            var questionGuideSub = await DMSQuestionGuideSubs.SingleOrDefaultAsync(a => a.SubId == id);
            Guard.Against.NoRecordPermission(questionGuideSub != null);

            if (questionGuideSub == null) return;

            questionGuideSub.UpdatedBy = userName;
            questionGuideSub.LastUpdate = DateTime.Now;

            int childId = questionGuideSub.ChildId;
            int oldIndex = questionGuideSub.OrderOfEntry;

            var questionGuideChild = await DMSQuestionGuideChildren.Where(w => w.ChildId == childId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(questionGuideChild != null);

            if (questionGuideChild == null) return;

            questionGuideChild.UpdatedBy = questionGuideSub.UpdatedBy;
            questionGuideChild.LastUpdate = questionGuideSub.LastUpdate;

            List<DMSQuestionGuideSub> questionGuideSubs = new List<DMSQuestionGuideSub>();
            if (oldIndex > newIndex)
            {
                questionGuideSubs = await DMSQuestionGuideSubs.Where(w => w.ChildId == childId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                questionGuideSubs.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                questionGuideSubs = await DMSQuestionGuideSubs.Where(w => w.ChildId == childId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                questionGuideSubs.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            questionGuideSub.OrderOfEntry = newIndex;
            questionGuideSubs.Add(questionGuideSub);

            _repository.Set<DMSQuestionGuideSub>().UpdateRange(questionGuideSubs);
            _repository.DMSQuestionGuideChildren.Update(questionGuideChild);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetQuestionGuideSubNextOrderOfEntry(int childId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await DMSQuestionGuideSubs.Where(ma => ma.ChildId == childId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        protected async Task UpdateSubParentStampsAsync(int childId, string userName)
        {
            var guideChild = await _repository.DMSQuestionGuideChildren.Where(w => w.ChildId == childId).FirstOrDefaultAsync();

            if (guideChild != null)
            {
                guideChild.UpdatedBy = userName;
                guideChild.LastUpdate = DateTime.Now;

                var entity = _repository.DMSQuestionGuideChildren.Attach(guideChild);
                entity.Property(c => c.UpdatedBy).IsModified = true;
                entity.Property(c => c.LastUpdate).IsModified = true;

                var questionGuide = await _repository.DMSQuestionGuides.Where(d => d.QuestionId == guideChild.QuestionId).FirstOrDefaultAsync();
                if (questionGuide != null)
                {
                    questionGuide.UpdatedBy = userName;
                    questionGuide.LastUpdate = DateTime.Now;

                    var guideEntity = _repository.DMSQuestionGuides.Attach(questionGuide);
                    guideEntity.Property(c => c.UpdatedBy).IsModified = true;
                    guideEntity.Property(c => c.LastUpdate).IsModified = true;

                    var guideGroup = await _repository.DMSQuestionGroups.Where(d => d.GroupId == questionGuide.GroupId).FirstOrDefaultAsync();
                    if (guideGroup != null)
                    {
                        guideGroup.UpdatedBy = userName;
                        guideGroup.LastUpdate = DateTime.Now;

                        var groupEntity = _repository.DMSQuestionGroups.Attach(guideGroup);
                        groupEntity.Property(c => c.UpdatedBy).IsModified = true;
                        groupEntity.Property(c => c.LastUpdate).IsModified = true;
                    }
                }
            }
        }
        #endregion

        #region Question Guide Sub Detail - only for Selection-choices for sub
        public async Task QuestionGuideSubDtlUpdate(int parentId, string userName, IEnumerable<DMSQuestionGuideSubDtl> updated, IEnumerable<DMSQuestionGuideSubDtl> added, IEnumerable<DMSQuestionGuideSubDtl> deleted)
        {
            if (updated.Any())
            {
                _repository.Set<DMSQuestionGuideSubDtl>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetQuestionGuideSubDtlNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<DMSQuestionGuideSubDtl>().AddRange(added);
            }

            if (deleted.Any())
            {                
                _repository.Set<DMSQuestionGuideSubDtl>().RemoveRange(deleted);
            }

            await UpdateSubDtlParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task ReorderQuestionGuideSubDtl(int id, string userName, int newIndex)
        {
            var questionGuideSubDtl = await DMSQuestionGuideSubDtls.SingleOrDefaultAsync(a => a.SubDtlId == id);
            Guard.Against.NoRecordPermission(questionGuideSubDtl != null);

            if (questionGuideSubDtl == null) return;

            questionGuideSubDtl.UpdatedBy = userName;
            questionGuideSubDtl.LastUpdate = DateTime.Now;

            int subId = questionGuideSubDtl.SubId;
            int oldIndex = questionGuideSubDtl.OrderOfEntry;

            var questionGuideSub = await DMSQuestionGuideSubs.Where(w => w.SubId == subId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(questionGuideSub != null);

            if (questionGuideSub == null) return;

            questionGuideSub.UpdatedBy = questionGuideSubDtl.UpdatedBy;
            questionGuideSub.LastUpdate = questionGuideSubDtl.LastUpdate;

            List<DMSQuestionGuideSubDtl> questionGuideSubDtls = new List<DMSQuestionGuideSubDtl>();
            if (oldIndex > newIndex)
            {
                questionGuideSubDtls = await DMSQuestionGuideSubDtls.Where(w => w.SubId == subId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                questionGuideSubDtls.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                questionGuideSubDtls = await DMSQuestionGuideSubDtls.Where(w => w.SubId == subId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                questionGuideSubDtls.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            questionGuideSubDtl.OrderOfEntry = newIndex;
            questionGuideSubDtls.Add(questionGuideSubDtl);

            _repository.Set<DMSQuestionGuideSubDtl>().UpdateRange(questionGuideSubDtls);
            _repository.DMSQuestionGuideSubs.Update(questionGuideSub);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetQuestionGuideSubDtlNextOrderOfEntry(int subId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await DMSQuestionGuideSubDtls.Where(ma => ma.SubId == subId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        protected async Task UpdateSubDtlParentStampsAsync(int subId, string userName)
        {
            var guideSub = await _repository.DMSQuestionGuideSubs.Where(w => w.SubId == subId).FirstOrDefaultAsync();

            if (guideSub != null)
            {
                guideSub.UpdatedBy = userName;
                guideSub.LastUpdate = DateTime.Now;

                var entity = _repository.DMSQuestionGuideSubs.Attach(guideSub);
                entity.Property(c => c.UpdatedBy).IsModified = true;
                entity.Property(c => c.LastUpdate).IsModified = true;

                var questionGuideChild = await _repository.DMSQuestionGuideChildren.Where(d => d.ChildId == guideSub.ChildId).FirstOrDefaultAsync();
                if (questionGuideChild != null)
                {
                    questionGuideChild.UpdatedBy = userName;
                    questionGuideChild.LastUpdate = DateTime.Now;

                    var guideChildEntity = _repository.DMSQuestionGuideChildren.Attach(questionGuideChild);
                    guideChildEntity.Property(c => c.UpdatedBy).IsModified = true;
                    guideChildEntity.Property(c => c.LastUpdate).IsModified = true;

                    var questionGuide = await _repository.DMSQuestionGuides.Where(d => d.QuestionId == questionGuideChild.QuestionId).FirstOrDefaultAsync();
                    if (questionGuide != null)
                    {
                        questionGuide.UpdatedBy = userName;
                        questionGuide.LastUpdate = DateTime.Now;

                        var guideEntity = _repository.DMSQuestionGuides.Attach(questionGuide);
                        guideEntity.Property(c => c.UpdatedBy).IsModified = true;
                        guideEntity.Property(c => c.LastUpdate).IsModified = true;

                        var guideGroup = await _repository.DMSQuestionGroups.Where(d => d.GroupId == questionGuide.GroupId).FirstOrDefaultAsync();
                        if (guideGroup != null)
                        {
                            guideGroup.UpdatedBy = userName;
                            guideGroup.LastUpdate = DateTime.Now;

                            var groupEntity = _repository.DMSQuestionGroups.Attach(guideGroup);
                            groupEntity.Property(c => c.UpdatedBy).IsModified = true;
                            groupEntity.Property(c => c.LastUpdate).IsModified = true;
                        }
                    }
                }                
            }
        }
        #endregion
    }
}

