using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ChildImageViewModel 
    {

        public int ParentId { get; set; }

        public DetailPagePermission? Permission { get; set; }

        public string? Area { get; set; }
        public string? Controller { get; set; }
        public string? ActivePage { get; set; } //parent javascript page

        public string? DocumentLink { get; set; } //for Folders icon link to Shared/Documents/ - SystemType|ScreenCode|DataKey|DataKeyValue - ex: P|Inv|InvId|1 (case sensitive, check tblDocControlMatterTree to get correct values)
        public bool AllowDownload { get; set; }
        public bool HasScreenSource { get; set; }
        public int FolderId { get; set; }
        public string? RoleLink { get; set; }

        // SharePoint
        public string? SharePointDocLibrary { get; set; }
        public string? SharePointDocLibraryFolder { get; set; }
        public string? SharePointRecKey { get; set; }

        public string? FolderName { get; set; }
    }
}
