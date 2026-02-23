using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDMSQuestionnaireService
    {
        IQueryable<DMSQuestionGroup> DMSQuestionGroups { get; }
        IQueryable<DMSQuestionGuide> DMSQuestionGuides { get; }
        IQueryable<DMSQuestionGuideChild> DMSQuestionGuideChildren { get; }
        IQueryable<DMSQuestionGuideSub> DMSQuestionGuideSubs { get; }
        IQueryable<DMSQuestionGuideSubDtl> DMSQuestionGuideSubDtls { get; }

        Task AddQuestionGroup(DMSQuestionGroup questionGroup);
        Task UpdateQuestionGroup(DMSQuestionGroup questionGroup);
        Task DeleteQuestionGroup(DMSQuestionGroup questionGroup);
        Task Copy(int oldGroupId, int newGroupId, string userName, bool copyQuestions);    
        Task GenerateNewQuestions(int questionId);

        Task UpdateChild<T>(int parentId, string userName, IEnumerable<DMSQuestionGuide> updated, IEnumerable<DMSQuestionGuide> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteQuestionGuide(int parentId, string userName, IEnumerable<DMSQuestionGuide> deleted);
        Task ReorderQuestionGuide(int id, string userName, int newIndex);

        Task QuestionGuideChildUpdate(int parentId, string userName, IEnumerable<DMSQuestionGuideChild> updated, IEnumerable<DMSQuestionGuideChild> added, IEnumerable<DMSQuestionGuideChild> deleted);
        Task ReorderQuestionGuideChild(int id, string userName, int newIndex);        

        Task QuestionGuideSubUpdate(int parentId, string userName, IEnumerable<DMSQuestionGuideSub> updated, IEnumerable<DMSQuestionGuideSub> added, IEnumerable<DMSQuestionGuideSub> deleted);
        Task ReorderQuestionGuideSub(int id, string userName, int newIndex);  
        
        Task QuestionGuideSubDtlUpdate(int parentId, string userName, IEnumerable<DMSQuestionGuideSubDtl> updated, IEnumerable<DMSQuestionGuideSubDtl> added, IEnumerable<DMSQuestionGuideSubDtl> deleted);
        Task ReorderQuestionGuideSubDtl(int id, string userName, int newIndex);
    }
}
