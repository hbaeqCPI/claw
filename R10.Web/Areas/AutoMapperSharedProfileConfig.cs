using AutoMapper;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Entities.Documents;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Web.Models.IManageModels;
using System.Linq;
using Microsoft.Graph;

namespace R10.Web.Areas
{
    public class AutoMapperSharedProfileConfig : Profile
    {
        public AutoMapperSharedProfileConfig()
        {
            #region Shared Auxiliaries

            CreateMap<ContactPerson, ContactSearchResultViewModel>();

            #endregion

            #region Documents
            CreateMap<Invention, DocumentInventionResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(i => "InvId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(i => i.InvId));

            CreateMap<CountryApplication, DocumentCtryAppResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "AppId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(a => a.AppId));

            CreateMap<TmkTrademark, DocumentTrademarkResultsViewModel>()
               .ForMember(vm => vm.DataKey, domain => domain.MapFrom(i => "TmkId"))
               .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(i => i.TmkId));

            CreateMap<PatActionDue, DocumentActionResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "ActId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(a => a.ActId))
                .ForMember(vm => vm.Status, domain => domain.MapFrom(a => a.CountryApplication.ApplicationStatus));

            CreateMap<TmkActionDue, DocumentActionResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "ActId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(a => a.ActId))
                .ForMember(vm => vm.Status, domain => domain.MapFrom(a => a.TmkTrademark.TrademarkStatus));

            CreateMap<PatCostTrack, DocumentCostResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "CostTrackId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(c => c.CostTrackId));

            CreateMap<TmkCostTrack, DocumentCostResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "CostTrackId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(c => c.CostTrackId));

            CreateMap<DocDocument, DocDocumentListViewModel>()
                .ForMember(vm => vm.DocId, domain => domain.MapFrom(d => d.DocId))
                .ForMember(vm => vm.DocName, domain => domain.MapFrom(d => d.DocName ?? ""))
                .ForMember(vm => vm.UserFileName, domain => domain.MapFrom(d => d.DocFile.UserFileName))
                .ForMember(vm => vm.DocFileName, domain => domain.MapFrom(d => d.DocFile.DocFileName))
                .ForMember(vm => vm.DateCreated, domain => domain.MapFrom(d => d.LastUpdate))
                .ForMember(vm => vm.DocTypeName, domain => domain.MapFrom(d => d.DocType != null ? d.DocType.DocTypeName : ""))
                .ForMember(vm => vm.ThumbFileName, domain => domain.MapFrom(d => d.DocFile.ThumbFileName ?? (!string.IsNullOrEmpty(d.DocUrl) ? "logo_url.png" : null)))
                .ForMember(vm => vm.IsPrivate, domain => domain.MapFrom(d => d.IsPrivate))
                .ForMember(vm => vm.LockedBy, domain => domain.MapFrom(d => d.LockedBy ?? ""))
                .ForMember(vm => vm.tStamp, domain => domain.MapFrom(d => d.tStamp))
                .ForMember(vm => vm.FolderId, domain => domain.MapFrom(d => d.FolderId))
                .ForMember(vm => vm.CreatedBy, domain => domain.MapFrom(d => d.CreatedBy))
                .ForMember(vm => vm.ScreenCode, domain => domain.MapFrom(d => d.DocFolder.ScreenCode))
                .ForMember(vm => vm.ParentId, domain => domain.MapFrom(d => d.DocFolder != null ? d.DocFolder.DataKeyValue : 0))
                .ForMember(vm => vm.DocUrl, domain => domain.MapFrom(d => d.DocUrl))
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(d => d.DocFolder.SystemType))
                .ForMember(vm => vm.FolderIsPublic, domain => domain.MapFrom(d => !d.DocFolder.IsPrivate))
                .ForMember(vm => vm.FolderName, domain => domain.MapFrom(d => d.DocFolder.FolderName))
                .ForMember(vm => vm.FolderCreatedBy, domain => domain.MapFrom(d => d.DocFolder.CreatedBy))
                .ForMember(vm => vm.IconClass, domain => domain.MapFrom(d => d.DocFile.DocIcon.IconClass))
                .ForMember(vm => vm.Tags, domain => domain.MapFrom(d => d.DocDocumentTags.Select(t => t.Tag)))
                .ForMember(vm => vm.ForSignature, domain => domain.MapFrom(d => d.DocFile.ForSignature))
                .ForMember(vm => vm.SignedDoc, domain => domain.MapFrom(d => d.DocFile.SignedDoc))
                .ForMember(vm => vm.EnvelopeId, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.EnvelopeId ?? ""))
                .ForMember(vm => vm.SignatureCompleted, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.SignatureCompleted ?? false))
                .ForMember(vm => vm.SentToDocuSign, domain => domain.MapFrom(d => !string.IsNullOrEmpty(d.DocFile.DocFileSignature.EnvelopeId)))
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.DataKey))
                .ForMember(vm => vm.QESetupId, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.QESetupId))
                .ForMember(vm => vm.RoleLink, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.RoleLink))
                .ForMember(vm => vm.SignatureReviewed, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.SignatureReviewed))
                .ForMember(vm => vm.UploadedDate, domain => domain.MapFrom(d => d.DocFile.DateCreated))
                .ForMember(vm => vm.Source, domain => domain.MapFrom(d => d.Source));

            CreateMap<DocDocumentListViewModel, DocDocument>()
                .ForMember(m => m.DocFile, opt => opt.Ignore())
                .ForMember(m => m.DocType, opt => opt.Ignore());

            CreateMap<DocDocumentViewModel, DocDocument>()
                .ForMember(m => m.DocFile, opt => opt.Ignore())
                .ForMember(m => m.DocType, opt => opt.Ignore());

            CreateMap<DocDocument, DocDocumentViewModel>()
                .ForMember(vm => vm.DocTypeName, domain => domain.MapFrom(d => d.DocType.DocTypeName))
                .ForMember(vm => vm.DocFileName, domain => domain.MapFrom(d => d.DocFile.DocFileName))
                .ForMember(vm => vm.UserFileName, domain => domain.MapFrom(d => d.DocFile.UserFileName))
                .ForMember(vm => vm.ThumbFileName, domain => domain.MapFrom(d => d.DocFile.ThumbFileName))
                .ForMember(vm => vm.IsImage, domain => domain.MapFrom(d => d.DocFile.IsImage))
                .ForMember(vm => vm.FileSize, domain => domain.MapFrom(d => d.DocFile.FileSize))
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(d => d.DocFolder.SystemType))
                .ForMember(vm => vm.ScreenCode, domain => domain.MapFrom(d => d.DocFolder.ScreenCode))
                .ForMember(vm => vm.ParentId, domain => domain.MapFrom(d => d.DocFolder != null ? d.DocFolder.DataKeyValue : 0));

            //iManage
            CreateMap<DocDocument, DocumentViewModel>();
            CreateMap<DocumentViewModel, Document>();
            CreateMap<DocumentViewModel, DocDocumentViewModel>();

            //NetDocs
            CreateMap<DocDocument, R10.Web.Models.NetDocumentsModels.DocumentViewModel>();
            CreateMap<R10.Web.Models.NetDocumentsModels.DocumentViewModel, R10.Web.Models.NetDocumentsModels.Document>();
            CreateMap<R10.Web.Models.NetDocumentsModels.DocumentViewModel, DocDocumentViewModel>();

            CreateMap<ImageViewModel, DocDocument>()
                .ForMember(m => m.DocId, vm => vm.MapFrom(d => d.ImageId));

            CreateMap<DocFolderViewModel, DocFolder>()
                .ForMember(m => m.SystemType, opt => opt.Ignore())
                .ForMember(m => m.DataKey, opt => opt.Ignore())
                .ForMember(m => m.DataKeyValue, opt => opt.Ignore())
                .ForMember(m => m.ParentFolderId, opt => opt.Ignore());

            CreateMap<CountryApplication, DocCtryAppViewModel>()
                .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ca => ca.Invention.Client.ClientName))
                .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(ca => ca.Invention.Attorney1.AttorneyCode))
                .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(ca => ca.Invention.Attorney2.AttorneyCode))
                .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(ca => ca.Invention.Attorney3.AttorneyCode));

            CreateMap<Invention, DocInventionViewModel>()
                .ForMember(vm => vm.ClientName, domain => domain.MapFrom(i => i.Client.ClientName))
                .ForMember(vm => vm.OwnerName, domain => domain.MapFrom(i => i.Owners.FirstOrDefault().Owner.OwnerName))
                .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(i => i.Attorney1.AttorneyName))
                .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(i => i.Attorney2.AttorneyName))
                .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(i => i.Attorney3.AttorneyName));

            CreateMap<TmkTrademark, DocTrademarkViewModel>()
                .ForMember(vm => vm.ClientName, domain => domain.MapFrom(t => t.Client.ClientName))
                //.ForMember(vm => vm.OwnerName, domain => domain.MapFrom(t => t.Owner.OwnerName))
                //MULTIPLE OWNER TABLE??
                .ForMember(vm => vm.OwnerName, domain => domain.MapFrom(t => t.Owners.FirstOrDefault().Owner.OwnerName))
                .ForMember(vm => vm.AgentName, domain => domain.MapFrom(t => t.Agent.AgentName))
                .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(t => t.Attorney1.AttorneyName))
                .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(t => t.Attorney2.AttorneyName))
                .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(t => t.Attorney3.AttorneyName))
                .ForMember(vm => vm.Attorney4, domain => domain.MapFrom(t => t.Attorney4.AttorneyName))
                .ForMember(vm => vm.Attorney5, domain => domain.MapFrom(t => t.Attorney5.AttorneyName));

            CreateMap<PatActionDue, DocPatActViewModel>()
               .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ad => ad.CountryApplication.Invention.Client.ClientName))
               .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(ad => ad.CountryApplication.Invention.Attorney1.AttorneyCode))
               .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(ad => ad.CountryApplication.Invention.Attorney2.AttorneyCode))
               .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(ad => ad.CountryApplication.Invention.Attorney3.AttorneyCode))
               .ForMember(vm => vm.AppNumber, domain => domain.MapFrom(ad => ad.CountryApplication.AppNumber))
               .ForMember(vm => vm.FilDate, domain => domain.MapFrom(ad => ad.CountryApplication.FilDate))
               .ForMember(vm => vm.PatNumber, domain => domain.MapFrom(ad => ad.CountryApplication.PatNumber))
               .ForMember(vm => vm.IssDate, domain => domain.MapFrom(ad => ad.CountryApplication.IssDate))
               .ForMember(vm => vm.CaseType, domain => domain.MapFrom(ad => ad.CountryApplication.CaseType))
               .ForMember(vm => vm.Status, domain => domain.MapFrom(ad => ad.CountryApplication.ApplicationStatus))
               .ForMember(vm => vm.AppTitle, domain => domain.MapFrom(ad => ad.CountryApplication.AppTitle));

            CreateMap<TmkActionDue, DocTmkActViewModel>()
              .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ad => ad.TmkTrademark.Client.ClientName))
              .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney1.AttorneyCode))
              .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney2.AttorneyCode))
              .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney3.AttorneyCode))
              .ForMember(vm => vm.Attorney4, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney4.AttorneyCode))
              .ForMember(vm => vm.Attorney5, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney5.AttorneyCode))
              .ForMember(vm => vm.AppNumber, domain => domain.MapFrom(ad => ad.TmkTrademark.AppNumber))
              .ForMember(vm => vm.FilDate, domain => domain.MapFrom(ad => ad.TmkTrademark.FilDate))
              .ForMember(vm => vm.RegNumber, domain => domain.MapFrom(ad => ad.TmkTrademark.RegNumber))
              .ForMember(vm => vm.RegDate, domain => domain.MapFrom(ad => ad.TmkTrademark.RegDate))
              .ForMember(vm => vm.CaseType, domain => domain.MapFrom(ad => ad.TmkTrademark.CaseType))
              .ForMember(vm => vm.Status, domain => domain.MapFrom(ad => ad.TmkTrademark.TrademarkStatus))
              .ForMember(vm => vm.TrademarkName, domain => domain.MapFrom(ad => ad.TmkTrademark.TrademarkName));

            CreateMap<PatCostTrack, DocPatCostViewModel>()
              .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ct => ct.CountryApplication.Invention.Client.ClientName))
              .ForMember(vm => vm.AppNumber, domain => domain.MapFrom(ct => ct.CountryApplication.AppNumber))
              .ForMember(vm => vm.FilDate, domain => domain.MapFrom(ct => ct.CountryApplication.FilDate))
              .ForMember(vm => vm.PatNumber, domain => domain.MapFrom(ct => ct.CountryApplication.PatNumber))
              .ForMember(vm => vm.IssDate, domain => domain.MapFrom(ct => ct.CountryApplication.IssDate))
              .ForMember(vm => vm.CaseType, domain => domain.MapFrom(ct => ct.CountryApplication.CaseType))
              .ForMember(vm => vm.Status, domain => domain.MapFrom(ct => ct.CountryApplication.ApplicationStatus))
              .ForMember(vm => vm.AppTitle, domain => domain.MapFrom(ct => ct.CountryApplication.AppTitle));

            CreateMap<TmkCostTrack, DocTmkCostViewModel>()
              .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ct => ct.TmkTrademark.Client.ClientName))
              .ForMember(vm => vm.AppNumber, domain => domain.MapFrom(ct => ct.TmkTrademark.AppNumber))
              .ForMember(vm => vm.FilDate, domain => domain.MapFrom(ct => ct.TmkTrademark.FilDate))
              .ForMember(vm => vm.RegNumber, domain => domain.MapFrom(ct => ct.TmkTrademark.RegNumber))
              .ForMember(vm => vm.RegDate, domain => domain.MapFrom(ct => ct.TmkTrademark.RegDate))
              .ForMember(vm => vm.CaseType, domain => domain.MapFrom(ct => ct.TmkTrademark.CaseType))
              .ForMember(vm => vm.Status, domain => domain.MapFrom(ct => ct.TmkTrademark.TrademarkStatus))
              .ForMember(vm => vm.TrademarkName, domain => domain.MapFrom(ct => ct.TmkTrademark.TrademarkName));

            CreateMap<DocFixedFolder, DocFixedFolderViewModel>();
            CreateMap<DocFolder, DocFolderViewModel>();
            #endregion

            #region Data Query
            #endregion
            #region Forms Extraction
            CreateMap<FormIFWActMap, FormIFWActionMapViewModel>()
                .ForMember(vm => vm.DocDesc, domain => domain.MapFrom(f => f.FormIFWDocType.DocDesc))
                .ForMember(vm => vm.IsGenActionDE, domain => domain.MapFrom(f => f.IsGenAction))
                .ForMember(vm => vm.IsCompareDE, domain => domain.MapFrom(f => f.IsCompare))
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(f => f.FormIFWDocType.SystemType));

            CreateMap<FormIFWActionMapViewModel, FormIFWActMap>()
               .ForMember(dom => dom.IsCompare, dom => dom.MapFrom(vm => vm.IsCompareDE))
               .ForMember(dom => dom.IsGenAction, dom => dom.MapFrom(vm => vm.IsGenActionDE))
               .ForMember(dom => dom.FormIFWDocType, opt => opt.Ignore());

            CreateMap<PatActionParameter, FormIFWDueDateViewModel>();

            #endregion

            #region Time Tracker
            #endregion


            CreateMap<SharePointGraphDriveItemViewModel, SharePointDocumentViewModel>()
               .ForMember(vm => vm.Folder, domain => domain.MapFrom(m => m.Path))
               .ForMember(vm => vm.Id, domain => domain.MapFrom(m => m.DriveItem.Id))
               //.ForMember(vm => vm.Title, domain => domain.MapFrom(m => m.DriveItem.Title))
               .ForMember(vm => vm.Name, domain => domain.MapFrom(m => m.DriveItem.Name))
               .ForMember(vm => vm.DateCreated_Offset, domain => domain.MapFrom(m => m.DriveItem.CreatedDateTime))
               .ForMember(vm => vm.DateModified_Offset, domain => domain.MapFrom(m => m.DriveItem.LastModifiedDateTime))
               .ForMember(vm => vm.CreatedBy, domain => domain.MapFrom(m => m.DriveItem.CreatedBy.User.AdditionalData["email"]))
               // .ForMember(vm => vm.CreatedBy, domain => domain.MapFrom(m => m.DriveItem.CreatedBy.User.DisplayName))
               .ForMember(vm => vm.ModifiedBy, domain => domain.MapFrom(m => m.DriveItem.LastModifiedBy == null ? "" : m.DriveItem.LastModifiedBy.User.DisplayName))
               .ForMember(vm => vm.ListItemId, domain => domain.MapFrom(m => m.DriveItem.ListItem.Id))
               .ForMember(vm => vm.ListItemFields, domain => domain.MapFrom(m => m.DriveItem.ListItem.Fields.AdditionalData))
               //.ForMember(vm => vm.EditUrl, domain => domain.MapFrom(m => m.DriveItem.EditUrl))
               //.ForMember(vm => vm.IsCheckedOut, domain => domain.MapFrom(m => m.DriveItem.IsCheckedOut))
               //.ForMember(vm => vm.CheckOutUser, domain => domain.MapFrom(m => m.DriveItem.CheckOutUser))
               //.ForMember(vm => vm.DownloadUrl, domain => domain.MapFrom(m => m.DriveItem.DownloadUrl))
               //.ForMember(vm => vm.PreviewUrl, domain => domain.MapFrom(m => m.DriveItem.PreviewUrl))
               .ForMember(vm => vm.EditUrl, domain => domain.MapFrom(m => m.DriveItem.WebUrl));

            #region Document Verification
            CreateMap<DocVerification, DocVerificationViewModel>();
            CreateMap<DocVerificationViewModel, DocVerification>()
                .ForMember(m => m.DocDocument, opt => opt.Ignore());
            CreateMap<DocVerificationDetail, DocVerification>()
                .ForMember(m => m.DocDocument, opt => opt.Ignore());

            CreateMap<DocumentVerificationSearchCriteriaDTO, DocumentVerificationSearchCriteriaViewModel>()
               .ForMember(vm => vm.Patent, opt => opt.Ignore())
               .ForMember(vm => vm.Trademark, opt => opt.Ignore())
               .ForMember(vm => vm.GeneralMatter, opt => opt.Ignore());

            CreateMap<DocumentVerificationSearchCriteriaViewModel, DocumentVerificationSearchCriteriaDTO>()
                .ForMember(dto => dto.DocName, vm => vm.MapFrom(c => c.DocNames != null && c.DocNames.Length > 0 ? "|" + string.Join("|", c.DocNames) + "|" : null))
                .ForMember(dto => dto.DocUploadedBy, vm => vm.MapFrom(c => c.DocUploadedBys != null && c.DocUploadedBys.Length > 0 ? "|" + string.Join("|", c.DocUploadedBys) + "|" : null))
                .ForMember(dto => dto.Country, vm => vm.MapFrom(c => c.Countries != null && c.Countries.Length > 0 ? "|" + string.Join("|", c.Countries) + "|" : null))
                .ForMember(dto => dto.Source, vm => vm.MapFrom(c => c.Sources != null && c.Sources.Length > 0 ? "|" + string.Join("|", c.Sources) + "|" : null))
                .ForMember(dto => dto.ActCreatedBy, vm => vm.MapFrom(c => c.ActCreatedBys != null && c.ActCreatedBys.Length > 0 ? "|" + string.Join("|", c.ActCreatedBys) + "|" : null))
                .ForMember(dto => dto.Client, vm => vm.MapFrom(c => c.Clients != null && c.Clients.Length > 0 ? "|" + string.Join("|", c.Clients) + "|" : null))
                .ForMember(dto => dto.Attorney, vm => vm.MapFrom(c => c.Attorneys != null && c.Attorneys.Length > 0 ? "|" + string.Join("|", c.Attorneys) + "|" : null))
                .ForMember(dto => dto.ActionType, vm => vm.MapFrom(c => c.ActionTypes != null && c.ActionTypes.Length > 0 ? "|" + string.Join("|", c.ActionTypes) + "|" : null))
                .ForMember(dto => dto.FilterAtty, vm => vm.MapFrom(c => "|" + c.AttorneyFilter1 + "|" + c.AttorneyFilter2 + "|" + c.AttorneyFilter3 + "|" + c.AttorneyFilter4 + "|" + c.AttorneyFilter5 + "|" + c.AttorneyFilterR + "|" + c.AttorneyFilterD + "|" + c.AttorneyFilterRD + "|"))
                .ForMember(dto => dto.RespDocketing, vm => vm.MapFrom(c => c.RespDocketings != null && c.RespDocketings.Length > 0 ? "|" + string.Join("|", c.RespDocketings) + "|" : null))
                .ForMember(dto => dto.RespReporting, vm => vm.MapFrom(c => c.RespReportings != null && c.RespReportings.Length > 0 ? "|" + string.Join("|", c.RespReportings) + "|" : null));

            CreateMap<DocumentVerificationNewDTO, DocVerificationNewPrintViewModel>();
            CreateMap<DocumentVerificationDTO, DocVerificationDocPrintViewModel>();
            CreateMap<DocumentVerificationActionDTO, DocVerificationActionDocPrintViewModel>();
            CreateMap<DocumentVerificationCommunicationDTO, DocVerificationCommDocPrintViewModel>();
            CreateMap<DocVerificationReviewFilters, DocVerificationReviewFilterViewModel>();
            CreateMap<DocVerificationReviewFilterViewModel, DocVerificationReviewFilters>()
                .ForMember(vm => vm.CountryFilter, vm => vm.MapFrom(c => c.Countries != null && c.Countries.Length > 0 ? "|" + string.Join("|", c.Countries) + "|" : ""))
                .ForMember(vm => vm.ClientFilter, vm => vm.MapFrom(c => c.Clients != null && c.Clients.Length > 0 ? "|" + string.Join("|", c.Clients) + "|" : ""))
                .ForMember(vm => vm.CaseTypeFilter, vm => vm.MapFrom(c => c.CaseTypes != null && c.CaseTypes.Length > 0 ? "|" + string.Join("|", c.CaseTypes) + "|" : ""));
            #endregion

        }
    }



}
