using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Clearance;
using R10.Core.Entities.PatClearance;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ITmcQuestionnaireService
    {
        Task AddQuestionGroup(TmcQuestionGroup questionGroup);
        Task UpdateQuestionGroup(TmcQuestionGroup questionGroup);
        Task DeleteQuestionGroup(TmcQuestionGroup questionGroup);
        Task UpdateChild<T>(int parentId, string userName, IEnumerable<TmcQuestionGuide> updated, IEnumerable<TmcQuestionGuide> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteQuestionGuide(int parentId, string userName, IEnumerable<TmcQuestionGuide> deleted);


        Task<List<TmcQuestionGuide>> GetQuestionGuides(int GroupId);
        Task<TmcQuestionGuide> GetQuestionGuide(int QuestionId);
        Task QuestionGuideUpdate(TmcQuestionGuide questionGuide);
        Task ReorderQuestionGuide(int id, string userName, int newIndex);

        Task QuestionGuideChildUpdate<T>(int parentId, string userName, IEnumerable<TmcQuestionGuideChild> updated, IEnumerable<TmcQuestionGuideChild> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task ReorderQuestionGuideChild(int id, string userName, int newIndex);

        IQueryable<TmcQuestionGroup> TmcQuestionGroups { get; }
        IQueryable<TmcQuestionGuide> TmcQuestionGuides { get; }
        IQueryable<TmcQuestionGuideChild> TmcQuestionGuideChildren { get; }
    }
}
