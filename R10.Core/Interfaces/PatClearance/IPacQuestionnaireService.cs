using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.PatClearance;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IPacQuestionnaireService
    {
        Task AddQuestionGroup(PacQuestionGroup questionGroup);
        Task UpdateQuestionGroup(PacQuestionGroup questionGroup);
        Task DeleteQuestionGroup(PacQuestionGroup questionGroup);
        Task UpdateChild<T>(int parentId, string userName, IEnumerable<PacQuestionGuide> updated, IEnumerable<PacQuestionGuide> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteQuestionGuide(int parentId, string userName, IEnumerable<PacQuestionGuide> deleted);


        Task<List<PacQuestionGuide>> GetQuestionGuides(int GroupId);
        Task<PacQuestionGuide> GetQuestionGuide(int QuestionId);
        Task QuestionGuideUpdate(PacQuestionGuide questionGuide);
        Task ReorderQuestionGuide(int id, string userName, int newIndex);

        Task QuestionGuideChildUpdate<T>(int parentId, string userName, IEnumerable<PacQuestionGuideChild> updated, IEnumerable<PacQuestionGuideChild> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task ReorderQuestionGuideChild(int id, string userName, int newIndex);

        IQueryable<PacQuestionGroup> PacQuestionGroups { get; }
        IQueryable<PacQuestionGuide> PacQuestionGuides { get; }
        IQueryable<PacQuestionGuideChild> PacQuestionGuideChildren { get; }
    }
}
