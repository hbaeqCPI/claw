using R10.Core.DTOs;
using R10.Web.Models.EmailAddInModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using R10.Web.Services.EmailAddIn;

namespace R10.Web.Interfaces
{
    public interface IOutlookService
    {
        Task<OutlookEmail> GetEmailMessage(string olItemId, string accessToken);

        Task<List<OutlookAttachment>> GetEmailAttachments(string olItemId, string[] attachmentIDs, string accessToken);
        Task<RegisterClientResult> RegisterOutlookAddInClient(string email, string clientSecret); //Outlook Add-In Registration --Yin

        Task<bool> DeleteOutlookAddInClient(string email); //Delete Outlook Add-In registration
    }
}
