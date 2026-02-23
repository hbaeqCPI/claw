using System;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Entities.PatClearance;
using Microsoft.EntityFrameworkCore;
using R10.Core.Exceptions;
using System.Transactions;
using R10.Core.Entities.DMS;

namespace R10.Core.Services
{
    public class PacQuestionnaireService : IPacQuestionnaireService
    {
        private readonly IApplicationDbContext _repository;

        public PacQuestionnaireService(IApplicationDbContext repository)

        {
            _repository = repository;
        }

        public IQueryable<PacQuestionGroup> PacQuestionGroups => _repository.PacQuestionGroups.AsNoTracking();
        public IQueryable<PacQuestionGuide> PacQuestionGuides => _repository.PacQuestionGuides;
        public IQueryable<PacQuestionGuideChild> PacQuestionGuideChildren => _repository.PacQuestionGuideChildren;

        public async Task AddQuestionGroup(PacQuestionGroup questionGroup)
        {
            _repository.PacQuestionGroups.Add(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateQuestionGroup(PacQuestionGroup questionGroup)
        {
            _repository.PacQuestionGroups.Update(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteQuestionGroup(PacQuestionGroup questionGroup)
        {
            _repository.PacQuestionGroups.Remove(questionGroup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateChild<T>(int parentId, string userName, IEnumerable<PacQuestionGuide> updated, IEnumerable<PacQuestionGuide> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            var addToDMSList = new List<PacQuestionGuide>();

            if (updated.Any())
            {
                _repository.Set<PacQuestionGuide>().UpdateRange(updated);

                addToDMSList.AddRange(updated.Where(u => u.AddCurrent && !u.RequiredOnSubmission).ToList());
            }               

            if (added.Any())
            {                
                var startIndex = await GetQuestionNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<PacQuestionGuide>().AddRange(added);

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
        public async Task DeleteQuestionGuide(int parentId, string userName, IEnumerable<PacQuestionGuide> deleted)
        {
            if (deleted.Any())
            {
                await UpdateChild(parentId, userName, new List<PacQuestionGuide>(), new List<PacQuestionGuide>(), deleted);
            }
        }

        public async Task<List<PacQuestionGuide>> GetQuestionGuides(int GroupId)
        {
            return await _repository.PacQuestionGuides.Where(c => c.GroupId == GroupId).AsNoTracking().ToListAsync();
        }

        public async Task<PacQuestionGuide> GetQuestionGuide(int QuestionId)
        {
            var questionGuide = await _repository.PacQuestionGuides.Where(c => c.QuestionId == QuestionId)
                                              .AsNoTracking().Include(c => c.PacQuestionGroup).FirstOrDefaultAsync();
            return questionGuide;
        }

        public async Task QuestionGuideUpdate(PacQuestionGuide questionGuide)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {

                if (questionGuide.QuestionId > 0)
                {
                    _repository.PacQuestionGuides.Update(questionGuide);
                }
                else
                {
                    _repository.PacQuestionGuides.Add(questionGuide);
                }

                await UpdateParentStampsAsync(questionGuide.GroupId, questionGuide.UpdatedBy);

                await _repository.SaveChangesAsync(); //we need to get the QuestionId

                scope.Complete();
            }
        }

        public async Task ReorderQuestionGuide(int id, string userName, int newIndex)
        {
            var questionGuide = await PacQuestionGuides.SingleOrDefaultAsync(a => a.QuestionId == id);
            Guard.Against.NoRecordPermission(questionGuide != null);
            questionGuide.UpdatedBy = userName;
            questionGuide.LastUpdate = DateTime.Now;

            int groupId = questionGuide.GroupId;
            int oldIndex = questionGuide.OrderOfEntry;

            var questionGroup = await PacQuestionGroups.Where(w => w.GroupId == groupId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(questionGroup != null);
            questionGroup.UpdatedBy = questionGuide.UpdatedBy;
            questionGroup.LastUpdate = questionGuide.LastUpdate;

            List<PacQuestionGuide> questionGuides = new List<PacQuestionGuide>();
            if (oldIndex > newIndex)
            {
                questionGuides = await PacQuestionGuides.Where(w => w.GroupId == groupId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                questionGuides.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                questionGuides = await PacQuestionGuides.Where(w => w.GroupId == groupId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                questionGuides.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            questionGuide.OrderOfEntry = newIndex;
            questionGuides.Add(questionGuide);

            _repository.Set<PacQuestionGuide>().UpdateRange(questionGuides);
            _repository.PacQuestionGroups.Update(questionGroup);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetQuestionNextOrderOfEntry(int groupId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await PacQuestionGuides.Where(ma => ma.GroupId == groupId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        protected async Task UpdateParentStampsAsync(int groupId, string userName)
        {
            var questionGroup = await _repository.PacQuestionGroups.Where(w => w.GroupId == groupId).FirstOrDefaultAsync();
            //var questionGroup = new PacQuestionGroup() { GroupId = groupId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            questionGroup.UpdatedBy = userName;
            questionGroup.LastUpdate = DateTime.Now;

            var entity = _repository.PacQuestionGroups.Attach(questionGroup);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
        #endregion

        #region Question Guide Child
        public async Task QuestionGuideChildUpdate<T>(int parentId, string userName, IEnumerable<PacQuestionGuideChild> updated, IEnumerable<PacQuestionGuideChild> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            if (updated.Any())
            {
                _repository.Set<PacQuestionGuideChild>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetQuestionGuideChildNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<PacQuestionGuideChild>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateChildParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task ReorderQuestionGuideChild(int id, string userName, int newIndex)
        {
            var questionGuideChild = await PacQuestionGuideChildren.SingleOrDefaultAsync(a => a.ChildId == id);
            Guard.Against.NoRecordPermission(questionGuideChild != null);
            questionGuideChild.UpdatedBy = userName;
            questionGuideChild.LastUpdate = DateTime.Now;

            int questionId = questionGuideChild.QuestionId;
            int oldIndex = questionGuideChild.OrderOfEntry;

            var questionGuide = await PacQuestionGuides.Where(w => w.QuestionId == questionId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(questionGuide != null);
            questionGuide.UpdatedBy = questionGuideChild.UpdatedBy;
            questionGuide.LastUpdate = questionGuideChild.LastUpdate;

            List<PacQuestionGuideChild> questionGuideChildren = new List<PacQuestionGuideChild>();
            if (oldIndex > newIndex)
            {
                questionGuideChildren = await PacQuestionGuideChildren.Where(w => w.QuestionId == questionId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                questionGuideChildren.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                questionGuideChildren = await PacQuestionGuideChildren.Where(w => w.QuestionId == questionId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                questionGuideChildren.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            questionGuideChild.OrderOfEntry = newIndex;
            questionGuideChildren.Add(questionGuideChild);

            _repository.Set<PacQuestionGuideChild>().UpdateRange(questionGuideChildren);
            _repository.PacQuestionGuides.Update(questionGuide);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetQuestionGuideChildNextOrderOfEntry(int questionId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await PacQuestionGuideChildren.Where(ma => ma.QuestionId == questionId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        protected async Task UpdateChildParentStampsAsync(int questionId, string userName)
        {
            var questionGuide = await _repository.PacQuestionGuides.Where(w => w.QuestionId == questionId).FirstOrDefaultAsync();
            questionGuide.UpdatedBy = userName;
            questionGuide.LastUpdate = DateTime.Now;

            var entity = _repository.PacQuestionGuides.Attach(questionGuide);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
        #endregion

        protected async Task GenerateNewQuestions(PacQuestionGuide questionGuide)
        {
            //To list of dmsId to add new question - not yet submitted, question is not required, and dms record doesn't have this question yet
            var tmcIds = await _repository.PacClearances.Where(d => d.ClearanceStatus.ToLower() != "submitted" 
                                                                && !d.PacQuestions.Any(q => q.QuestionId == questionGuide.QuestionId))
                                                        .Select(d => d.PacId).ToListAsync();

            if (tmcIds.Count > 0)
            {
                foreach (var tmcId in tmcIds)
                {
                    var newPacQuestion = new PacQuestion()
                    {
                        PacId = tmcId,
                        QuestionId = questionGuide.QuestionId,
                        CreatedBy = questionGuide.CreatedBy,
                        UpdatedBy = questionGuide.UpdatedBy,
                        DateCreated = questionGuide.DateCreated,
                        LastUpdate = questionGuide.LastUpdate
                    };
                    _repository.Set<PacQuestion>().Add(newPacQuestion);
                }
                await _repository.SaveChangesAsync();
            }
        }
    }
}

