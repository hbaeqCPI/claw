using System;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Entities.Clearance;
using Microsoft.EntityFrameworkCore;
using R10.Core.Exceptions;
using System.Transactions;
using R10.Core.Entities.PatClearance;

namespace R10.Core.Services
{
    public class TmcQuestionnaireService : ITmcQuestionnaireService
    {
        private readonly IApplicationDbContext _repository;

        public TmcQuestionnaireService(IApplicationDbContext repository)

        {
            _repository = repository;
        }

        public IQueryable<TmcQuestionGroup> TmcQuestionGroups => _repository.TmcQuestionGroups.AsNoTracking();
        public IQueryable<TmcQuestionGuide> TmcQuestionGuides => _repository.TmcQuestionGuides;
        public IQueryable<TmcQuestionGuideChild> TmcQuestionGuideChildren => _repository.TmcQuestionGuideChildren;


        public async Task AddQuestionGroup(TmcQuestionGroup questionGroup)
        {
            _repository.TmcQuestionGroups.Add(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateQuestionGroup(TmcQuestionGroup questionGroup)
        {
            _repository.TmcQuestionGroups.Update(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteQuestionGroup(TmcQuestionGroup questionGroup)
        {
            _repository.TmcQuestionGroups.Remove(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateChild<T>(int parentId, string userName, IEnumerable<TmcQuestionGuide> updated, IEnumerable<TmcQuestionGuide> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            var addToDMSList = new List<TmcQuestionGuide>();

            if (updated.Any())
            {
                _repository.Set<TmcQuestionGuide>().UpdateRange(updated);

                addToDMSList.AddRange(updated.Where(u => u.AddCurrent && !u.RequiredOnSubmission).ToList());
            }               

            if (added.Any())
            {                
                var startIndex = await GetQuestionNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<TmcQuestionGuide>().AddRange(added);

                addToDMSList.AddRange(added.Where(a => a.AddCurrent && !a.RequiredOnSubmission).ToList());
            }                

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();

            if (addToDMSList.Count > 0)
            {
                //Add new questions to existing DMS records
                foreach(var questionGuide in addToDMSList)
                {
                    await GenerateNewQuestions(questionGuide);
                }
            }
        }

        #region Question Guide
        public async Task DeleteQuestionGuide(int parentId, string userName, IEnumerable<TmcQuestionGuide> deleted)
        {
            if (deleted.Any())
            {
                await UpdateChild(parentId, userName, new List<TmcQuestionGuide>(), new List<TmcQuestionGuide>(), deleted);
            }
        }

        public async Task<List<TmcQuestionGuide>> GetQuestionGuides(int GroupId)
        {
            return await _repository.TmcQuestionGuides.Where(c => c.GroupId == GroupId).AsNoTracking().ToListAsync();
        }

        public async Task<TmcQuestionGuide> GetQuestionGuide(int QuestionId)
        {
            var questionGuide = await _repository.TmcQuestionGuides.Where(c => c.QuestionId == QuestionId)
                                              .AsNoTracking().Include(c => c.TmcQuestionGroup).FirstOrDefaultAsync();
            return questionGuide;
        }

        public async Task QuestionGuideUpdate(TmcQuestionGuide questionGuide)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {

                if (questionGuide.QuestionId > 0)
                {
                    _repository.TmcQuestionGuides.Update(questionGuide);
                }
                else
                {
                    _repository.TmcQuestionGuides.Add(questionGuide);
                }

                await UpdateParentStampsAsync(questionGuide.GroupId, questionGuide.UpdatedBy);

                await _repository.SaveChangesAsync(); //we need to get the QuestionId

                scope.Complete();
            }
        }

        public async Task ReorderQuestionGuide(int id, string userName, int newIndex)
        {
            var questionGuide = await TmcQuestionGuides.SingleOrDefaultAsync(a => a.QuestionId == id);
            Guard.Against.NoRecordPermission(questionGuide != null);
            questionGuide.UpdatedBy = userName;
            questionGuide.LastUpdate = DateTime.Now;

            int groupId = questionGuide.GroupId;
            int oldIndex = questionGuide.OrderOfEntry;

            var questionGroup = await TmcQuestionGroups.Where(w => w.GroupId == groupId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(questionGroup != null);
            questionGroup.UpdatedBy = questionGuide.UpdatedBy;
            questionGroup.LastUpdate = questionGuide.LastUpdate;

            List<TmcQuestionGuide> questionGuides = new List<TmcQuestionGuide>();
            if (oldIndex > newIndex)
            {
                questionGuides = await TmcQuestionGuides.Where(w => w.GroupId == groupId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                questionGuides.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                questionGuides = await TmcQuestionGuides.Where(w => w.GroupId == groupId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                questionGuides.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            questionGuide.OrderOfEntry = newIndex;
            questionGuides.Add(questionGuide);

            _repository.Set<TmcQuestionGuide>().UpdateRange(questionGuides);
            _repository.TmcQuestionGroups.Update(questionGroup);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetQuestionNextOrderOfEntry(int groupId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await TmcQuestionGuides.Where(ma => ma.GroupId == groupId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        protected async Task UpdateParentStampsAsync(int groupId, string userName)
        {
            var questionGroup = await _repository.TmcQuestionGroups.Where(w => w.GroupId == groupId).FirstOrDefaultAsync();
            //var questionGroup = new TmcQuestionGroup() { GroupId = groupId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            questionGroup.UpdatedBy = userName;
            questionGroup.LastUpdate = DateTime.Now;

            var entity = _repository.TmcQuestionGroups.Attach(questionGroup);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
        #endregion

        #region Question Guide Child
        public async Task QuestionGuideChildUpdate<T>(int parentId, string userName, IEnumerable<TmcQuestionGuideChild> updated, IEnumerable<TmcQuestionGuideChild> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            if (updated.Any())
            {
                _repository.Set<TmcQuestionGuideChild>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetQuestionGuideChildNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<TmcQuestionGuideChild>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateChildParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task ReorderQuestionGuideChild(int id, string userName, int newIndex)
        {
            var questionGuideChild = await TmcQuestionGuideChildren.SingleOrDefaultAsync(a => a.ChildId == id);
            Guard.Against.NoRecordPermission(questionGuideChild != null);
            questionGuideChild.UpdatedBy = userName;
            questionGuideChild.LastUpdate = DateTime.Now;

            int questionId = questionGuideChild.QuestionId;
            int oldIndex = questionGuideChild.OrderOfEntry;

            var questionGuide = await TmcQuestionGuides.Where(w => w.QuestionId == questionId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(questionGuide != null);
            questionGuide.UpdatedBy = questionGuideChild.UpdatedBy;
            questionGuide.LastUpdate = questionGuideChild.LastUpdate;

            List<TmcQuestionGuideChild> questionGuideChildren = new List<TmcQuestionGuideChild>();
            if (oldIndex > newIndex)
            {
                questionGuideChildren = await TmcQuestionGuideChildren.Where(w => w.QuestionId == questionId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                questionGuideChildren.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                questionGuideChildren = await TmcQuestionGuideChildren.Where(w => w.QuestionId == questionId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                questionGuideChildren.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            questionGuideChild.OrderOfEntry = newIndex;
            questionGuideChildren.Add(questionGuideChild);

            _repository.Set<TmcQuestionGuideChild>().UpdateRange(questionGuideChildren);
            _repository.TmcQuestionGuides.Update(questionGuide);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetQuestionGuideChildNextOrderOfEntry(int questionId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await TmcQuestionGuideChildren.Where(ma => ma.QuestionId == questionId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        protected async Task UpdateChildParentStampsAsync(int questionId, string userName)
        {
            var questionGuide = await _repository.TmcQuestionGuides.Where(w => w.QuestionId == questionId).FirstOrDefaultAsync();
            questionGuide.UpdatedBy = userName;
            questionGuide.LastUpdate = DateTime.Now;

            var entity = _repository.TmcQuestionGuides.Attach(questionGuide);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
        #endregion       

        protected async Task GenerateNewQuestions(TmcQuestionGuide questionGuide)
        {
            //To list of dmsId to add new question - not yet submitted, question is not required, and dms record doesn't have this question yet
            var tmcIds = await _repository.TmcClearances.Where(d => d.ClearanceStatus.ToLower() != "submitted" 
                                                                && !d.TmcQuestions.Any(q => q.QuestionId == questionGuide.QuestionId))
                                                        .Select(d => d.TmcId).ToListAsync();

            if (tmcIds.Count > 0)
            {
                foreach (var tmcId in tmcIds)
                {
                    var newTmcQuestion = new TmcQuestion()
                    {
                        TmcId = tmcId,
                        QuestionId = questionGuide.QuestionId,
                        CreatedBy = questionGuide.CreatedBy,
                        UpdatedBy = questionGuide.UpdatedBy,
                        DateCreated = questionGuide.DateCreated,
                        LastUpdate = questionGuide.LastUpdate
                    };
                    _repository.Set<TmcQuestion>().Add(newTmcQuestion);
                }
                await _repository.SaveChangesAsync();
            }
        }
    }
}

