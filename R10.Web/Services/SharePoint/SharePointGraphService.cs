using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.Web.Server.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using DocuSign.eSign.Model;
using GleamTech.IO;
using iText.Layout.Element;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Marketplace.CorporateCuratedGallery;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using System;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace R10.Web.Services.SharePoint
{
    // GraphServiceClient extensions for SharePoint
    // and other SharePoint helpers
    public static class SharePointGraphService
    {
        public static async Task<Microsoft.Graph.Site> GetSiteByPath(this GraphServiceClient graphClient, string siteRelativePath, string hostname)
        {
            return await graphClient.Sites.GetByPath(siteRelativePath, hostname).Request().Expand("drives").GetAsync();
        }

        public static async Task<Microsoft.Graph.Site> GetSiteWithLists(this GraphServiceClient graphClient, string siteRelativePath, string hostname)
        {
            return await graphClient.Sites.GetByPath(siteRelativePath, hostname).Request().Expand("lists").GetAsync();
        }


        public static async Task<List<SharePointGraphDriveItemViewModel>> GetSiteDocuments(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);
                var items = await graphClient.GetDriveItems(drive.Id, folders);
                foreach (var item in items)
                {
                    if (item.Path != path && item.Path.StartsWith(path))
                    {
                        item.Path = item.Path.Substring(path.Length + 1);
                    }
                    else if (item.Path == path)
                    {
                        item.Path = "";
                    }
                }

                return items;
            }
            return new List<SharePointGraphDriveItemViewModel>();
        }


        public static async Task<List<SharePointGraphDriveItemViewModel>> GetSiteDocumentsByMetadata(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string screen, string recKey)
        {
            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

            var output = new List<SharePointGraphDriveItemViewModel>();

            try
            {
                var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPIScreen eq '{screen}' and fields/CPIRecordKey eq '{recKey}'").Expand("driveItem").GetAsync();
                if (result.CurrentPage.Count > 0)
                {
                    var process = true;
                    var items = result.CurrentPage;

                    while (process)
                    {
                        foreach (var item in items)
                        {
                            var driveItem = item.DriveItem;
                            driveItem.ListItem = item;
                            output.Add(new SharePointGraphDriveItemViewModel { Path = "", DriveItem = driveItem });
                        }

                        process = false;
                        if (result.NextPageRequest != null)
                        {
                            var page = await result.NextPageRequest.GetAsync();
                            if (page.CurrentPage.Count > 0)
                            {
                                result = page;
                                items = page.CurrentPage;
                                process = true;
                            }
                        }

                    }
                    return output;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return new List<SharePointGraphDriveItemViewModel>();
        }

        public static async Task<List<SharePointGraphDriveItemViewModel>> GetSiteDocuments(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string itemId, bool isFolder)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var result = new List<SharePointGraphDriveItemViewModel>();
                try
                {
                    if (isFolder)
                    {
                        var driveItems = await graphClient.Drives[drive.Id].Items[itemId].Children.Request().Expand("listItem").GetAsync();
                        foreach (var item in driveItems)
                        {
                            if (item.Folder is null)
                            {
                                result.Add(new SharePointGraphDriveItemViewModel { Path = "", DriveItem = item });
                            }
                            else
                            {
                                var items = await GetDriveItems(graphClient, drive.Id, item.Id, item.Name);
                                result.AddRange(items.Select(d => new SharePointGraphDriveItemViewModel { Path = d.Path, DriveItem = d.DriveItem }).ToList());
                            }
                        }
                    }
                    else
                    {
                        var driveItem = await graphClient.Drives[drive.Id].Items[itemId].Request().Expand("listItem").GetAsync();
                        if (driveItem != null)
                            result.Add(new SharePointGraphDriveItemViewModel { Path = "", DriveItem = driveItem });
                    }

                    return result;
                }
                catch (ServiceException ex)
                {
                    return result;
                }
            }
            return null;
        }

        public static async Task<List<SharePointGraphDocPicklistViewModel>> GetSiteDocumentNames(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders, string? filter)
        {
            var result = new List<SharePointGraphDocPicklistViewModel>();

            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
            if (list != null)
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);
                var listItems = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Expand("driveItem").GetAsync();
                if (listItems.CurrentPage.Count > 0)
                {
                    var url = list.WebUrl + "/" + path.Replace(" ", "%20");

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter = filter.Replace("*", "").Replace("%", "");
                        filter = filter.ToLower();
                    }

                    var process = true;
                    var items = listItems.CurrentPage;

                    while (process)
                    {
                        foreach (var item in items)
                        {
                            if (item.WebUrl.StartsWith(url) && item.DriveItem.Folder is null)
                            {

                                if (filter != null && filter.Length > 0 && !item.DriveItem.Name.ToLower().Contains(filter))
                                    continue;

                                var pathFolders = item.WebUrl.Replace(list.WebUrl, "").Split("/").Where(f => f.Length > 0).ToList();
                                var filePath = string.Join("/", pathFolders);

                                var isPrivate = false;
                                var fields = item.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                                if (fields.ContainsKey("IsPrivate"))
                                    isPrivate = Convert.ToBoolean(fields.GetValueOrDefault("IsPrivate").ToString());

                                //var modifiedBy = item.LastModifiedBy.User != null ? item.LastModifiedBy.User.DisplayName : item.CreatedBy.User.DisplayName;
                                var modifiedBy = item.DriveItem.CreatedBy.User != null ? item.DriveItem.CreatedBy.User.AdditionalData["email"].ToString().Left(20) : "";
                                var lastModified = item.LastModifiedDateTime != null ? item.LastModifiedDateTime.Value.DateTime : item.CreatedDateTime.Value.DateTime;

                                result.Add(new SharePointGraphDocPicklistViewModel
                                {
                                    Id = item.DriveItem.Id,
                                    Folder = filePath,
                                    DocName = item.DriveItem.Name,
                                    ModifiedBy = modifiedBy,
                                    DateModified = lastModified,
                                    IsPrivate = isPrivate,
                                });
                            }
                        }
                        process = false;
                        if (listItems.NextPageRequest != null)
                        {
                            var page = await listItems.NextPageRequest.GetAsync();
                            if (page.CurrentPage.Count > 0)
                            {
                                listItems = page;
                                items = page.CurrentPage;
                                process = true;
                            }
                        }

                    }

                }
            }
            return result;

        }

        public static async Task<List<SharePointGraphDocPicklistViewModel>> GetSiteDocumentNamesByMetadata(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string screen, string? filter)
        {
            var result = new List<SharePointGraphDocPicklistViewModel>();

            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
            if (list != null)
            {
                try
                {
                    var criteria = $"fields/CPIScreen eq '{screen}'";
                    var listItems = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter(criteria).Expand("driveItem").GetAsync();
                    if (listItems.CurrentPage.Count > 0)
                    {
                        var process = true;
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter = filter.Replace("*", "").Replace("%", "");
                            filter = filter.ToLower();
                        }

                        var items = listItems.CurrentPage;
                        while (process)
                        {
                            foreach (var item in items)
                            {
                                if (filter != null && filter.Length > 0 && !item.DriveItem.Name.ToLower().Contains(filter))
                                    continue;

                                var isPrivate = false;
                                var fields = item.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                                if (fields.ContainsKey("IsPrivate"))
                                    isPrivate = Convert.ToBoolean(fields.GetValueOrDefault("IsPrivate").ToString());

                                //var modifiedBy = item.LastModifiedBy.User != null ? item.LastModifiedBy.User.DisplayName : item.CreatedBy.User.DisplayName;
                                var modifiedBy = item.DriveItem.CreatedBy.User != null ? item.DriveItem.CreatedBy.User.AdditionalData["email"].ToString().Left(20) : "";
                                var lastModified = item.LastModifiedDateTime != null ? item.LastModifiedDateTime.Value.DateTime : item.CreatedDateTime.Value.DateTime;

                                result.Add(new SharePointGraphDocPicklistViewModel
                                {
                                    Id = item.DriveItem.Id,
                                    Folder = "",
                                    RecKey = fields.GetValueOrDefault("CPIRecordKey").ToString(),
                                    DocName = item.DriveItem.Name,
                                    ModifiedBy = modifiedBy,
                                    DateModified = lastModified,
                                    IsPrivate = isPrivate
                                });
                            }
                            process = false;
                            if (listItems.NextPageRequest != null)
                            {
                                var page = await listItems.NextPageRequest.GetAsync();
                                if (page.CurrentPage.Count > 0)
                                {
                                    listItems = page;
                                    items = page.CurrentPage;
                                    process = true;
                                }
                            }

                        }

                    }
                }
                catch (Exception ex)
                {

                    if (!ex.Message.Contains("itemNotFound"))
                        throw;
                }
            }
            return result;

        }

        public static async Task<List<string>> GetDocLibraryTags(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary)
        {
            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

            var output = new List<string>();
            var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Expand("fields($select=CPiTags)").GetAsync();
            if (result.CurrentPage.Count > 0)
            {
                var process = true;
                var items = result.CurrentPage;
                while (process)
                {
                    foreach (var item in items)
                    {
                        var fields = item.Fields;
                        if (fields.AdditionalData.TryGetValue("CPiTags", out var objTags))
                        {
                            var tags = objTags.ToString();
                            output.AddRange(tags.Split(";"));
                        }
                    }

                    process = false;
                    if (result.NextPageRequest != null)
                    {
                        var page = await result.NextPageRequest.GetAsync();
                        if (page.CurrentPage.Count > 0)
                        {
                            result = page;
                            items = page.CurrentPage;
                            process = true;
                        }
                    }
                }

            }
            return output;
        }

        public static async Task CopyDocuments(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string docLibraryFolder, string sourceRecKey, string dtnRecKey, string dtnParentKey = "")
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var sourceFolders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, sourceRecKey);
                sourceFolders = sourceFolders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var sourcePath = string.Join("/", sourceFolders);

                if (string.IsNullOrEmpty(dtnParentKey) || SharePointViewModelService.IsSharePointRecKeySingleNodeOnly)
                {
                    try
                    {
                        await graphClient.Drives[drive.Id].Root.ItemWithPath(sourcePath).Copy(dtnRecKey).Request().PostAsync();
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("itemNotFound"))
                            throw;
                    }

                }
                else
                {
                    var parentFolders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, dtnParentKey);
                    parentFolders = parentFolders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                    var parentPath = string.Join("/", parentFolders);

                    var parentId = string.Empty;
                    try
                    {
                        var parent = await graphClient.Drives[drive.Id].Root.ItemWithPath(parentPath).Request().GetAsync();
                        parentId = parent.Id;

                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("itemNotFound"))
                        {
                            var driveItemToCreate = new DriveItem
                            {
                                Name = dtnParentKey,
                                Folder = new Microsoft.Graph.Folder(),
                            };
                            var parent = await graphClient.Drives[drive.Id].Root.ItemWithPath(docLibraryFolder).Children.Request().AddAsync(driveItemToCreate);
                            parentId = parent.Id;
                        }
                        else throw;
                    }

                    if (!string.IsNullOrEmpty(parentId))
                    {
                        var parentReference = new ItemReference
                        {
                            Id = parentId
                        };
                        await graphClient.Drives[drive.Id].Root.ItemWithPath(sourcePath).Copy(dtnRecKey, parentReference).Request().PostAsync();
                    }
                }
            }

        }

        public static async Task<List<SharePointSyncCopyDTO>> CopyDocumentsByMetadata(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string screen, string sourceRecKey, string dtnRecKey, string dtnParentKey = "")
        {
            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);

            var docs = new List<SharePointSyncCopyDTO>();
            try
            {
                var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPIScreen eq '{screen}' and fields/CPIRecordKey eq '{sourceRecKey}'").Expand("driveItem").GetAsync();
                if (result.CurrentPage.Count > 0)
                {
                    var process = true;
                    var items = result.CurrentPage;

                    while (process)
                    {
                        foreach (var source in items)
                        {
                            var sourceDriveItem = source.DriveItem;
                            var newFileName = Path.GetFileNameWithoutExtension(sourceDriveItem.Name) + "-" + dtnRecKey + Path.GetExtension(sourceDriveItem.Name);
                            //var parentReference = new ItemReference
                            //{
                            //    DriveId = drive.Id,
                            //    //Id = target folderid
                            //};
                            var copyResponse = await graphClient.Drives[drive.Id].Items[sourceDriveItem.Id].Copy(newFileName).Request().PostResponseAsync();
                            if (copyResponse.HttpHeaders.TryGetValues("Location", out var headerValues))
                            {
                                string locationString = headerValues?.First();
                                Regex rx = new Regex(@"items/(.*?)\?");
                                var driveItemId = rx.Match(locationString).Groups[1].Value;

                                var requestBody = new FieldValueSet
                                {
                                    AdditionalData = new Dictionary<string, object>
                                            {
                                                {
                                                    "CPIRecordKey",dtnRecKey
                                                }
                                            }
                                };
                                if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                                {
                                    requestBody.AdditionalData.Add("CPISyncCompleted", true);
                                }
                                var driveItem = await graphClient.Drives[drive.Id].Items[driveItemId].Request().Expand("listItem").GetAsync();
                                if (driveItem != null)
                                {
                                    var updateResult = await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                                    var doc = new SharePointSyncCopyDTO
                                    {
                                        DocLibrary = docLibrary,
                                        Screen = screen,
                                        SourceDriveItemId = sourceDriveItem.Id,
                                        NewDriveItemId = driveItemId,
                                        NewFileName = newFileName
                                    };
                                    docs.Add(doc);
                                }
                            }

                        }

                        process = false;
                        if (result.NextPageRequest != null)
                        {
                            var page = await result.NextPageRequest.GetAsync();
                            if (page.CurrentPage.Count > 0)
                            {
                                result = page;
                                items = page.CurrentPage;
                                process = true;
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return docs;
        }

        public static async Task<bool> HasDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);
                try
                {
                    var driveItems = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().GetAsync();
                    return driveItems.Count > 0;
                }
                catch (ServiceException ex)
                {
                    return false;
                }
            }
            return false;
        }

        public static async Task<bool> HasDriveItemByMetadata(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string screen, string recKey)
        {
            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

            try
            {
                var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPIScreen eq '{screen}' and fields/CPIRecordKey eq '{recKey}'").GetAsync();
                return result.CurrentPage.Count > 0;

            }
            catch (Exception ex)
            {
                //throw
            }
            return false;
        }


        public static async Task<List<SharePointGraphTreeViewModel>> GetDriveItemsTree(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);
                var result = new List<SharePointGraphTreeViewModel>();
                try
                {
                    var driveItems = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().GetAsync();
                    foreach (var item in driveItems)
                    {
                        if (item.Folder is null)
                        {
                            result.Add(new SharePointGraphTreeViewModel { Id = item.Id, Name = item.Name });
                        }
                        else
                        {
                            result.Add(new SharePointGraphTreeViewModel { Id = item.Id, Name = item.Name, IsFolder = true });
                        }
                    }
                    return result;
                }
                catch (ServiceException ex)
                {
                    return result;
                }
            }
            return null;
        }

        public static async Task<List<SharePointGraphTreeViewModel>> GetDriveItemsTreeByMetadata(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string screen, string recKey)
        {

            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

            var output = new List<SharePointGraphTreeViewModel>();
            try
            {
                var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPIScreen eq '{screen}' and fields/CPIRecordKey eq '{recKey}'").Expand("driveItem").GetAsync();
                if (result.CurrentPage.Count > 0)
                {
                    var process = true;
                    var items = result.CurrentPage;

                    while (process)
                    {
                        foreach (var item in items)
                        {
                            output.Add(new SharePointGraphTreeViewModel { Id = item.DriveItem.Id, Name = item.DriveItem.Name });
                        }

                        process = false;
                        if (result.NextPageRequest != null)
                        {
                            var page = await result.NextPageRequest.GetAsync();
                            if (page.CurrentPage.Count > 0)
                            {
                                result = page;
                                items = page.CurrentPage;
                                process = true;
                            }
                        }

                    }
                    return output;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return new List<SharePointGraphTreeViewModel>();

        }


        public static async Task<List<SharePointGraphTreeViewModel>> GetDriveItemsTree(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string itemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var result = new List<SharePointGraphTreeViewModel>();
                try
                {
                    var driveItems = await graphClient.Drives[drive.Id].Items[itemId].Children.Request().GetAsync();
                    foreach (var item in driveItems)
                    {
                        if (item.Folder is null)
                        {
                            result.Add(new SharePointGraphTreeViewModel { Id = item.Id, Name = item.Name });
                        }
                        else
                        {
                            result.Add(new SharePointGraphTreeViewModel { Id = item.Id, Name = item.Name, IsFolder = true });
                        }
                    }
                    return result;
                }
                catch (ServiceException ex)
                {
                    return result;
                }
            }
            return null;
        }

        public static async Task<bool> FolderHasChildren(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);
                var result = new List<SharePointGraphTreeViewModel>();
                try
                {
                    var driveItems = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().GetAsync();
                    return driveItems.Count() > 0;
                }
                catch (ServiceException ex)
                {
                    return false;
                }
            }
            return false;
        }

        public static async Task<bool> FolderHasChildren(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string itemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var result = new List<SharePointGraphTreeViewModel>();
                try
                {
                    var driveItems = await graphClient.Drives[drive.Id].Items[itemId].Children.Request().GetAsync();
                    return driveItems.Count() > 0;
                }
                catch (ServiceException ex)
                {
                    return false;
                }
            }
            return false;
        }

        public static async Task<DriveItem> CreateSiteLibraryFolder(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string parentItemId, string folderName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {

                var driveItemToCreate = new DriveItem
                {
                    Name = folderName,
                    Folder = new Microsoft.Graph.Folder(),
                };

                try
                {
                    return await graphClient.Drives[drive.Id].Items[parentItemId].Children.Request().AddAsync(driveItemToCreate);
                }
                catch (ServiceException ex)
                {
                    if (!ex.Message.Contains("nameAlreadyExists"))
                        throw;
                }
                return null;
            }
            return null;
        }

        public static async Task<DriveItem> CreateSiteLibraryFolder(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null && folders.Count > 0)
            {
                var basePath = "root";
                if (folders.Count > 1)
                {
                    folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                    var parentFolders = folders.Take(folders.Count - 1).ToList();
                    basePath = string.Join("/", parentFolders);
                }

                var driveItemToCreate = new DriveItem
                {
                    Name = folders.Last(),
                    Folder = new Microsoft.Graph.Folder(),
                    //AdditionalData = new Dictionary<string, object>()
                    //{
                    //    {"@microsoft.graph.conflictBehavior", "replace"}
                    //}
                };

                try
                {
                    if (folders.Count > 1)
                        return await graphClient.Drives[drive.Id].Root.ItemWithPath(basePath).Children.Request().AddAsync(driveItemToCreate);
                    else
                        return await graphClient.Drives[drive.Id].Items[basePath].Children.Request().AddAsync(driveItemToCreate);
                }
                catch (ServiceException ex)
                {
                    if (!ex.Message.Contains("nameAlreadyExists"))
                        throw;
                }
                return null;
            }
            return null;
        }

        public static async Task<DriveItem> GetSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var result = await graphClient.Drives[drive.Id].Items[driveItemId].Request().Expand("listItem").GetAsync();
                return result;
            }
            return null;
        }

        public static async Task<DriveItem> GetSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string path, string fileName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var driveItems = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().GetAsync();
                foreach (var item in driveItems)
                {
                    if (item.Folder is null && item.Name.ToLower() == fileName.ToLower())
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public static async Task<bool> FileExists(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string fileName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var driveItems = await graphClient.Drives[drive.Id].Root.Children.Request().Filter($"Name eq '{fileName}'").GetAsync();
                return driveItems.Count > 0;
            }
            return false;
        }

        public static async Task<bool> FileExists(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders, string fileName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var path = string.Empty;
                if (folders.Count > 0)
                {
                    folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                    path = string.Join("/", folders);
                    path = $"{path}/{fileName}";
                }
                else path = fileName;

                try
                {
                    var driveItem = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().GetAsync();
                    if (driveItem != null)
                        return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public static async Task<bool> DeleteSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                await graphClient.Drives[drive.Id].Items[driveItemId].Request().DeleteAsync();
                return true;
            }
            return false;
        }

        public static async Task<bool> DeleteSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders, string fileName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);
                await graphClient.Drives[drive.Id].Root.ItemWithPath(path + "/" + fileName).Request().DeleteAsync();
                return true;
            }
            return false;
        }

        public static async Task<bool> RenameSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId, string newName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                await graphClient.Drives[drive.Id].Items[driveItemId].Request().UpdateAsync(new DriveItem { Name = newName });
                return true;
            }
            return false;
        }


        public static async Task RenameMainRecordKey(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string docLibraryFolder, string recKey, string newName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var path = "";
                var processInvention = docLibraryFolder == SharePointDocLibraryFolder.Invention;
                if (processInvention)
                {
                    var folders = new List<string> { SharePointDocLibraryFolder.Invention, recKey };
                    folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                    path = string.Join("/", folders);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

                var processApplication = processInvention || docLibraryFolder == SharePointDocLibraryFolder.Application;
                if (processApplication)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.Application, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

                var processInvAction = processInvention || docLibraryFolder == SharePointDocLibraryFolder.InventionAction;
                if (processInvAction)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.InventionAction, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }
                var processInvCost = processInvention || docLibraryFolder == SharePointDocLibraryFolder.InventionCostTracking;
                if (processInvCost)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.InventionCostTracking, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

                var processTrademark = docLibraryFolder == SharePointDocLibraryFolder.Trademark;
                if (processTrademark)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.Trademark, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

                var processGM = docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter;
                if (processGM)
                {
                    var folders = new List<string> { SharePointDocLibraryFolder.GeneralMatter, recKey };
                    folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                    path = string.Join("/", folders);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

                var processAction = processApplication || processTrademark || processGM || docLibraryFolder == SharePointDocLibraryFolder.Action;
                if (processAction)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.Action, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }
                var processCost = processApplication || processTrademark || processGM || docLibraryFolder == SharePointDocLibraryFolder.Cost;
                if (processCost)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.Cost, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

                var processDMS = docLibraryFolder == SharePointDocLibraryFolder.DMS;
                if (processDMS)
                {
                    var folders = new List<string> { SharePointDocLibraryFolder.DMS, recKey };
                    folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                    path = string.Join("/", folders);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }
                var processPatClearance = docLibraryFolder == SharePointDocLibraryFolder.PatClearance;
                if (processPatClearance)
                {
                    var folders = new List<string> { SharePointDocLibraryFolder.PatClearance, recKey };
                    folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                    path = string.Join("/", folders);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }
                var processTmkRequest = docLibraryFolder == SharePointDocLibraryFolder.TmkRequest;
                if (processTmkRequest)
                {
                    var folders = new List<string> { SharePointDocLibraryFolder.TmkRequest, recKey };
                    folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                    path = string.Join("/", folders);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

            }
        }

        public static async Task RenameRecordKeyByMetadata(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string screen, string oldKey, string newKey)
        {
            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

            try
            {
                var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPIRecordKey eq '{oldKey}' or startswith(fields/CPIRecordKey,'{oldKey}{SharePointSeparator.Field}')").Expand("fields").GetAsync();
                if (result.CurrentPage.Count > 0)
                {
                    var process = true;
                    var items = result.CurrentPage;

                    var newRecordKey = newKey;
                    while (process)
                    {
                        foreach (var item in items)
                        {
                            var fields = item.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                            if (fields.ContainsKey("CPIScreen") && fields.GetValueOrDefault("CPIScreen").ToString().ToLower() != screen.ToLower())
                            {
                                var oldRecordKey = fields.GetValueOrDefault("CPIRecordKey").ToString();
                                newRecordKey = oldRecordKey.Replace($"{oldKey}{SharePointSeparator.Field}", $"{newKey}{SharePointSeparator.Field}");
                            }

                            var requestBody = new FieldValueSet
                            {
                                AdditionalData = new Dictionary<string, object>
                                        {
                                            {
                                                "CPIRecordKey", newRecordKey
                                            }
                                        }
                            };
                            var updateResult = await graphClient.Sites[site.Id].Lists[list.Id].Items[item.Id].Fields.Request().UpdateAsync(requestBody);
                        }

                        process = false;
                        if (result.NextPageRequest != null)
                        {
                            var page = await result.NextPageRequest.GetAsync();
                            if (page.CurrentPage.Count > 0)
                            {
                                result = page;
                                items = page.CurrentPage;
                                process = true;
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public static async Task RenameRecordKey(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string docLibraryFolder, string recKey, string newName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {

                var path = "";
                var oldName = "";
                var processApplication = docLibraryFolder == SharePointDocLibraryFolder.Application;
                if (processApplication)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.Application, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }

                    oldName = recKey.Split(SharePointSeparator.Folder).Last();
                    var filter = $"startswith(Name,'{oldName}{SharePointSeparator.Field}')";
                    path = BuildFolderPath(SharePointDocLibraryFolder.Action, recKey.Split(SharePointSeparator.Folder)[0]);
                    try
                    {
                        var actions = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().Filter(filter).GetAsync();
                        await RenameDriveItems(graphClient, actions, drive.Id, oldName, newName);
                    }
                    catch (Exception ex)
                    {
                    }

                    path = BuildFolderPath(SharePointDocLibraryFolder.Cost, recKey.Split(SharePointSeparator.Folder)[0]);
                    try
                    {
                        var costs = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().Filter(filter).GetAsync();
                        await RenameDriveItems(graphClient, costs, drive.Id, oldName, newName);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                var processInvAction = docLibraryFolder == SharePointDocLibraryFolder.InventionAction;
                if (processInvAction)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.InventionAction, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }
                var processInvCost = docLibraryFolder == SharePointDocLibraryFolder.InventionCostTracking;
                if (processInvCost)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.InventionCostTracking, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

                var processTrademark = docLibraryFolder == SharePointDocLibraryFolder.Trademark;
                if (processTrademark)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.Trademark, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }

                    oldName = recKey.Split(SharePointSeparator.Folder).Last();
                    var filter = $"startswith(Name,'{oldName}{SharePointSeparator.Field}')";
                    path = BuildFolderPath(SharePointDocLibraryFolder.Action, recKey.Split(SharePointSeparator.Folder)[0]);
                    try
                    {
                        var actions = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().Filter(filter).GetAsync();
                        await RenameDriveItems(graphClient, actions, drive.Id, oldName, newName);
                    }
                    catch (Exception ex)
                    {
                    }

                    path = BuildFolderPath(SharePointDocLibraryFolder.Cost, recKey.Split(SharePointSeparator.Folder)[0]);
                    try
                    {
                        var costs = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().Filter(filter).GetAsync();
                        await RenameDriveItems(graphClient, costs, drive.Id, oldName, newName);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                var processGM = docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter;
                if (processGM)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.GeneralMatter, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }

                    oldName = recKey.Split(SharePointSeparator.Folder).Last();
                    var filter = $"startswith(Name,'{oldName}{SharePointSeparator.Field}')";
                    path = BuildFolderPath(SharePointDocLibraryFolder.Action, "");
                    try
                    {
                        var actions = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().Filter(filter).GetAsync();
                        await RenameDriveItems(graphClient, actions, drive.Id, oldName, newName);
                    }
                    catch (Exception ex)
                    {
                    }

                    path = BuildFolderPath(SharePointDocLibraryFolder.Cost, "");
                    try
                    {
                        var costs = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Children.Request().Filter(filter).GetAsync();
                        await RenameDriveItems(graphClient, costs, drive.Id, oldName, newName);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                var processAction = docLibraryFolder == SharePointDocLibraryFolder.Action;
                if (processAction)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.Action, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }
                var processCost = docLibraryFolder == SharePointDocLibraryFolder.Cost;
                if (processCost)
                {
                    path = BuildFolderPath(SharePointDocLibraryFolder.Cost, recKey);
                    try { await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Request().UpdateAsync(new DriveItem { Name = newName }); } catch (Exception) { }
                }

            }
        }

        private static string BuildFolderPath(string docLibraryFolder, string recKey)
        {
            var folders = new List<string> { docLibraryFolder };
            if (!string.IsNullOrEmpty(recKey))
                folders.AddRange(recKey.Split(SharePointSeparator.Folder).ToList());

            folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
            var path = string.Join("/", folders);
            return path;
        }

        private static async Task RenameDriveItems(GraphServiceClient graphClient, IDriveItemChildrenCollectionPage result, string driveId, string oldName, string newName)
        {
            if (result.CurrentPage.Count > 0)
            {
                var process = true;
                var items = result.CurrentPage;

                while (process)
                {
                    foreach (var item in items)
                    {
                        var updatedName = item.Name.Replace($"{oldName}{SharePointSeparator.Field}", $"{newName}{SharePointSeparator.Field}");
                        await graphClient.Drives[driveId].Items[item.Id].Request().UpdateAsync(new DriveItem { Name = updatedName });
                    }

                    process = false;
                    if (result.NextPageRequest != null)
                    {
                        var page = await result.NextPageRequest.GetAsync();
                        if (page.CurrentPage.Count > 0)
                        {
                            result = page;
                            items = page.CurrentPage;
                            process = true;
                        }
                    }

                }
            }
        }

        public static async Task<bool> MoveSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId, string parentDriveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                DriveItem? targetFolder = null;
                try
                {
                    targetFolder = await graphClient.Drives[drive.Id].Root.ItemWithPath(parentDriveItemId).Request().GetAsync();
                }
                catch (Exception ex) { }

                var requestBody = new DriveItem
                {
                    ParentReference = new ItemReference
                    {
                        Id = targetFolder != null ? targetFolder.Id : parentDriveItemId,
                    },
                    //Name = "new-item-name.txt",
                };
                await graphClient.Drives[drive.Id].Items[driveItemId].Request().UpdateAsync(requestBody);
                return true;
            }
            return false;
        }

        public static async Task<string> GetSiteDriveItemPreviewUrl(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var result = await graphClient.Drives[drive.Id].Items[driveItemId].Preview().Request().PostAsync();
                return result.GetUrl;
            }
            return string.Empty;
        }

        public static async Task<string> GetSiteDriveItemPreviewUrl(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders, string fileName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);
                var result = await graphClient.Drives[drive.Id].Root.ItemWithPath(path + "/" + fileName).Preview().Request().PostAsync();
                return result.GetUrl;
            }
            return string.Empty;
        }

        public static async Task GetSiteDriveItemsPreviewUrl(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<SharePointDocumentViewModel> images)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                foreach (var item in images)
                {
                    if (item.Name.EndsWith(".url"))
                    {
                        item.PreviewUrl = await graphClient.GetSiteDriveItemPreviewUrlForLink(siteRelativePath, hostname, docLibrary, item.Id);
                    }
                    else
                    {
                        var result = await graphClient.Drives[drive.Id].Items[item.Id].Preview().Request().PostAsync();
                        item.PreviewUrl = result.GetUrl;
                    }
                }
            }
        }

        public static async Task<string> GetSiteDriveItemPreviewUrlForLink(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string id)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var stream = await graphClient.Drives[drive.Id].Items[id].Content.Request().GetAsync();
                if (stream != null)
                {
                    var content = Encoding.UTF8.GetString(stream.ReadToEnd(), 0, (int)stream.Length);
                    var startPos = content.IndexOf("URL=");
                    if (startPos > 0)
                    {
                        return content.Substring(startPos + 4);
                    }
                }
            }
            return "";
        }

        public static async Task<SharePointGraphThumbnailViewModel> GetSiteDriveItemThumbnailUrl(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string itemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var result = await graphClient.Drives[drive.Id].Items[itemId].Thumbnails.Request().GetAsync();
                if (result != null && result.CurrentPage.Count > 0)
                {
                    return new SharePointGraphThumbnailViewModel
                    {
                        SmallThumbnailUrl = result.CurrentPage[0].Small.Url,
                        MediumThumbnailUrl = result.CurrentPage[0].Medium.Url,
                        BigThumbnailUrl = result.CurrentPage[0].Large.Url
                    };
                }
            }
            return null;
        }

        public static async Task GetSiteDriveItemsThumbnailUrl(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<SharePointDocumentViewModel> images)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                foreach (var item in images)
                {
                    var result = await graphClient.Drives[drive.Id].Items[item.Id].Thumbnails.Request().GetAsync();
                    if (result != null && result.CurrentPage.Count > 0)
                    {
                        item.ThumbnailUrl = result.CurrentPage[0].Small.Url;
                    }
                }
            }
        }

        public static async Task CheckoutSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                await graphClient.Drives[drive.Id].Items[driveItemId].Checkout().Request().PostAsync();
            }
        }

        public static async Task<bool> IsSiteDriveItemCheckedOut(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var driveItem = await graphClient.Drives[drive.Id].Items[driveItemId].Request().Expand("listItem").GetAsync();
                if (driveItem != null)
                {
                    var zz = driveItem.ListItem.Fields.AdditionalData.TryGetValue("CheckoutUserLookupId", out var checkoutUser);
                    if (checkoutUser != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static async Task CheckinSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                await graphClient.Drives[drive.Id].Items[driveItemId].Checkin().Request().PostAsync();
            }
        }

        public static async Task DownloadSiteDriveItems(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<SharePointDocumentDownloadViewModel> driveItems)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                foreach (var item in driveItems.Where(i => !i.Name.EndsWith(".url")).ToList())
                {
                    var stream = await graphClient.Drives[drive.Id].Items[item.DriveItemId].Content.Request().GetAsync();
                    if (stream != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            item.FileBytes = memoryStream.ToArray();
                        }
                    }
                }
            }
        }

        public static async Task DownloadSiteDriveItems(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string folder, List<SharePointDocumentDownloadViewModel> fileNames)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                foreach (var item in fileNames.Where(i => !i.Name.EndsWith(".url")).ToList())
                {
                    var path = folder + "/" + item.Name;
                    var stream = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Content.Request().GetAsync();
                    if (stream != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            item.FileBytes = memoryStream.ToArray();
                        }
                    }
                }
            }
        }

        public static async Task<Stream> DownloadSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var stream = await graphClient.Drives[drive.Id].Items[driveItemId].Content.Request().GetAsync();
                return stream;
            }
            return null;
        }

        public static async Task<Stream> DownloadSiteDriveItem(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string folder, string fileName)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var path = string.Empty;
                if (!string.IsNullOrEmpty(folder))
                    path = folder + "/" + fileName;
                else
                    path = fileName;

                var stream = await graphClient.Drives[drive.Id].Root.ItemWithPath(path).Content.Request().GetAsync();
                return stream;
            }
            return null;
        }

        public static async Task<List<SharePointGraphDriveItemVersionViewModel>> GetSiteDriveItemVersionHistory(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var history = await graphClient.Drives[drive.Id].Items[driveItemId].Versions.Request().GetAsync();
                if (history.CurrentPage.Count > 0)
                {
                    var list = new List<SharePointGraphDriveItemVersionViewModel>();

                    foreach (var item in history.CurrentPage)
                    {
                        var driveItemVersion = new SharePointGraphDriveItemVersionViewModel()
                        {
                            Id = item.Id,
                            DateModified = item.LastModifiedDateTime.HasValue ? item.LastModifiedDateTime.Value.DateTime : null,
                            ModifiedBy = item.LastModifiedBy.User.DisplayName,
                            Size = item.Size / 1000
                        };
                        list.Add(driveItemVersion);
                    }
                    return list;
                }
            }
            return new List<SharePointGraphDriveItemVersionViewModel>();
        }

        public static async Task<Stream> GetSiteDriveItemVersionContent(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId, string versionId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                var content = await graphClient.Drives[drive.Id].Items[driveItemId].Versions[versionId].Content.Request().GetAsync();
                return content;
            }
            return null;
        }

        public static async Task RestoreSiteDriveItemVersion(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId, string versionId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                await graphClient.Drives[drive.Id].Items[driveItemId].Versions[versionId].RestoreVersion().Request().PostAsync();
            }
        }

        public static async Task<Microsoft.Graph.ListItem> GetSiteUserInformation(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string siteId, string userId)
        {
            var userInfo = await graphClient.Sites[siteId].Lists["User Information List"].Items[userId].Request().GetAsync();
            return userInfo;
        }

        public static async Task<SharePointGraphDriveItemKeyViewModel> UploadSiteFile(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders, Stream fileStream, string fileName)
        {
            var result = new SharePointGraphDriveItemKeyViewModel();
            var site = await graphClient.Sites.GetByPath(siteRelativePath, hostname).Request().Expand("drives,lists").GetAsync();
            var drive = site.Drives.FirstOrDefault(d => d.Name == docLibrary);

            if (drive != null)
            {
                result.DriveId = drive.Id;
                var path = String.Empty;
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                if (folders.Count > 0)
                {
                    if (SharePointViewModelService.IsSharePointIntegrationUsingDocumentSet)
                    {
                        var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

                        var topFolder = folders[0];
                        DriveItem driveItem = null;
                        try
                        {
                            driveItem = await graphClient.Drives[drive.Id].Root.ItemWithPath(topFolder).Request().GetAsync();
                        }
                        //exception when it doesn't exists
                        catch (Exception ex)
                        {
                        }

                        if (driveItem == null) {

                            if (!SharePointViewModelService.IsSharePointIntegrationAutoAddDocumentSet) {
                                throw new Exception($"Document Set is missing for {topFolder}");
                            }

                            var contentType = (await GetSiteContentTypes(graphClient, site.Id, list.Id)).Where(c => c.Name.ToLower() == "document set").FirstOrDefault();
                            if (contentType == null)
                                throw new Exception("Document Library has no Document Set content type");

                            var driveItemToCreate = new DriveItem
                            {
                                Name = topFolder,
                                Folder = new Microsoft.Graph.Folder(),
                            };

                            var topFolderDriveItem = await graphClient.Drives[drive.Id].Root.Children.Request().AddAsync(driveItemToCreate);
                            var listItem = await graphClient.Drives[drive.Id].Items[topFolderDriveItem.Id].Request().Expand("listItem").GetAsync();
                            if (listItem != null)
                            {
                                var requestBody = new Microsoft.Graph.ListItem
                                {
                                    ContentType = new ContentTypeInfo { Id = contentType .Id},
                                    //Fields = new FieldValueSet {
                                    //    AdditionalData = new Dictionary<string, object>
                                    //{
                                    //    //{
                                    //    //    "Name" , "Ronald-test-docset1"
                                    //    //},
                                    //},
                                    //},
                                };
                                var updateResult = await graphClient.Sites[site.Id].Lists[list.Id].Items[listItem.ListItem.Id].Request().UpdateAsync(requestBody);
                            }
                        }

                    }

                    path = string.Join("/", folders);
                    path = $"{path}/{fileName}";
                }
                else path = fileName;

                //less than 4MB
                if (fileStream.Length < 4194304)
                {
                    result.DriveItemId = await graphClient.UploadSmallFile(drive.Id, fileStream, path);
                }
                else
                {
                    result.DriveItemId = await graphClient.UploadBigFile(drive.Id, fileStream, path);
                }
            }
            return result;
        }

        public static async Task<SharePointGraphDriveItemKeyViewModel> UploadSiteFile(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string parentItemId, Stream fileStream, string fileName)
        {
            var result = new SharePointGraphDriveItemKeyViewModel();
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);

            if (drive != null)
            {
                result.DriveId = drive.Id;

                //less than 4MB
                if (fileStream.Length < 4194304)
                {
                    result.DriveItemId = await graphClient.UploadSmallFile(drive.Id, fileStream, parentItemId, fileName);
                }
                else
                {
                    result.DriveItemId = await graphClient.UploadBigFile(drive.Id, fileStream, parentItemId, fileName);
                }
            }
            return result;
        }

        public static async Task<DriveItem> UpdateSiteFile(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId, Stream fileStream)
        {
            var result = false;
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                //less than 4MB
                if (fileStream.Length < 4194304)
                {
                    return await graphClient.Drives[drive.Id].Items[driveItemId].Content.Request().PutAsync<DriveItem>(fileStream);
                }
                else
                {
                    using (Stream stream = fileStream)
                    {
                        var uploadSession = await graphClient.Drives[drive.Id].Items[driveItemId].CreateUploadSession().Request().PostAsync();
                        // create upload task  
                        var maxChunkSize = 320 * 1024;
                        var largeUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, stream, maxChunkSize);

                        // upload file  
                        UploadResult<DriveItem> uploadResult = largeUploadTask.UploadAsync().Result;
                        return uploadResult.ItemResponse;
                    }
                }
            }
            return null;
        }

        public static async Task<DefaultImageViewModel> GetDefaultImage(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders, string? driveId = "")
        {
            if (string.IsNullOrEmpty(driveId))
            {
                var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
                driveId = drive.Id;
            }

            if (!string.IsNullOrEmpty(driveId))
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);

                try
                {
                    var driveItems = await graphClient.Drives[driveId].Root.ItemWithPath(path).Children.Request().Expand("listItem").GetAsync();
                    foreach (var item in driveItems)
                    {
                        if (item.Image != null)
                        {
                            var fields = item.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                            if (fields.ContainsKey("IsDefault"))
                            {
                                var isDefault = Convert.ToBoolean(fields.GetValueOrDefault("IsDefault").ToString());

                                if (isDefault && item.File.MimeType.Contains("image"))
                                {
                                    var isPrivate = false;
                                    var title = item.Name.Split(".")[0];

                                    if (fields.ContainsKey("IsPrivate"))
                                        isPrivate = Convert.ToBoolean(fields.GetValueOrDefault("IsPrivate").ToString());

                                    if (fields.ContainsKey("Title"))
                                        title = fields.GetValueOrDefault("Title").ToString();

                                    return new DefaultImageViewModel
                                    {
                                        SharePointDriveItemId = item.Id,
                                        SharePointDocLibrary = docLibrary,
                                        SharePointDriveId = driveId,
                                        ImageFile = item.Name,
                                        IsPublic = !isPrivate,
                                        ImageTitle = title
                                    };
                                }
                            }
                        }
                    }

                }
                catch (ServiceException)
                {
                    return null;
                }
            }
            return null;
        }

        public static async Task UnmarkDefaultImage(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, List<string> folders, string siteId, string listId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            var driveId = drive.Id;

            if (!string.IsNullOrEmpty(driveId))
            {
                folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
                var path = string.Join("/", folders);

                try
                {
                    var driveItems = await graphClient.Drives[driveId].Root.ItemWithPath(path).Children.Request().Expand("listItem").GetAsync();
                    foreach (var item in driveItems)
                    {
                        if (item.Image != null)
                        {
                            var fields = item.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                            if (fields.ContainsKey("IsDefault"))
                            {
                                var isDefault = Convert.ToBoolean(fields.GetValueOrDefault("IsDefault").ToString());
                                if (isDefault && item.File.MimeType.Contains("image"))
                                {
                                    var requestBody = new FieldValueSet
                                    {
                                        AdditionalData = new Dictionary<string, object>
                                        {
                                            {
                                                "IsDefault", false
                                            }
                                        }
                                    };
                                    var updateResult = await graphClient.Sites[siteId].Lists[listId].Items[item.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                                }
                            }
                        }
                    }

                }
                catch (ServiceException)
                {
                    //return;
                }
            }
        }

        public static async Task<DefaultImageViewModel> GetDefaultImageByMetadata(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string screen, string recKey)
        {
            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

            try
            {
                var result = await graphClient.Sites[site.Id].Lists[list.Id].Items.Request().Filter($"fields/CPIScreen eq '{screen}' and fields/CPIRecordKey eq '{recKey}'").Expand("driveItem").GetAsync();
                if (result.CurrentPage.Count > 0)
                {
                    var process = true;
                    var items = result.CurrentPage;

                    while (process)
                    {
                        foreach (var item in items)
                        {
                            if (item.DriveItem.Image != null)
                            {
                                var fields = item.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                                if (fields.ContainsKey("IsDefault"))
                                {
                                    var isDefault = Convert.ToBoolean(fields.GetValueOrDefault("IsDefault").ToString());

                                    if (isDefault && item.DriveItem.File.MimeType.Contains("image"))
                                    {
                                        var isPrivate = false;
                                        var title = item.DriveItem.Name.Split(".")[0];

                                        if (fields.ContainsKey("IsPrivate"))
                                            isPrivate = Convert.ToBoolean(fields.GetValueOrDefault("IsPrivate").ToString());

                                        if (fields.ContainsKey("Title"))
                                            title = fields.GetValueOrDefault("Title").ToString();

                                        return new DefaultImageViewModel
                                        {
                                            SharePointDriveItemId = item.DriveItem.Id,
                                            SharePointDocLibrary = docLibrary,
                                            //SharePointDriveId = driveId,
                                            ImageFile = item.DriveItem.Name,
                                            IsPublic = !isPrivate,
                                            ImageTitle = title
                                        };
                                    }
                                }
                            }
                        }

                        process = false;
                        if (result.NextPageRequest != null)
                        {
                            var page = await result.NextPageRequest.GetAsync();
                            if (page.CurrentPage.Count > 0)
                            {
                                result = page;
                                items = page.CurrentPage;
                                process = true;
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("itemNotFound"))
                    throw;
            }
            return null;
        }


        public static async Task UnmarkDefaultImageByMetadata(this GraphServiceClient graphClient, string siteId, string listId, string screen, string recKey)
        {
            try
            {
                var result = await graphClient.Sites[siteId].Lists[listId].Items.Request().Filter($"fields/CPIScreen eq '{screen}' and fields/CPIRecordKey eq '{recKey}'").Expand("driveItem").GetAsync();
                if (result.CurrentPage.Count > 0)
                {
                    var process = true;
                    var items = result.CurrentPage;

                    while (process)
                    {
                        foreach (var item in items)
                        {
                            if (item.DriveItem.Image != null)
                            {
                                var fields = item.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                                if (fields.ContainsKey("IsDefault"))
                                {
                                    var isDefault = Convert.ToBoolean(fields.GetValueOrDefault("IsDefault").ToString());

                                    if (isDefault && item.DriveItem.File.MimeType.Contains("image"))
                                    {
                                        var requestBody = new FieldValueSet
                                        {
                                            AdditionalData = new Dictionary<string, object>
                                        {
                                            {
                                                "IsDefault", false
                                            }
                                        }
                                        };
                                        var updateResult = await graphClient.Sites[siteId].Lists[listId].Items[item.Id].Fields.Request().UpdateAsync(requestBody);

                                    }
                                }
                            }
                        }

                        process = false;
                        if (result.NextPageRequest != null)
                        {
                            var page = await result.NextPageRequest.GetAsync();
                            if (page.CurrentPage.Count > 0)
                            {
                                result = page;
                                items = page.CurrentPage;
                                process = true;
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {

            }

        }

        public static async Task CreateDocumentLibrary(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary)
        {
            var site = await graphClient.GetSiteByPath(siteRelativePath, hostname);
            var list = new Microsoft.Graph.List
            {
                DisplayName = docLibrary,
                ListInfo = new ListInfo
                {
                    Template = "documentLibrary",
                    Hidden = false
                }
            };
            var result = await graphClient.Sites[site.Id].Lists.Request().AddAsync(list);
        }

        private static async Task<string> UploadSmallFile(this GraphServiceClient graphClient, string driveId, Stream fileStream, string path)
        {
            var uploadedFile = await graphClient.Drives[driveId].Root.ItemWithPath(path).Content.Request().PutAsync<DriveItem>(fileStream);
            return uploadedFile.Id;
        }

        private static async Task<string> UploadSmallFile(this GraphServiceClient graphClient, string driveId, Stream fileStream, string parentItemId, string fileName)
        {
            var uploadedFile = await graphClient.Drives[driveId].Items[parentItemId].ItemWithPath(fileName).Content.Request().PutAsync<DriveItem>(fileStream);
            return uploadedFile.Id;
        }

        private static async Task<string> UploadBigFile(this GraphServiceClient graphClient, string driveId, Stream fileStream, string path)
        {
            using (Stream stream = fileStream)
            {
                var uploadSession = await graphClient.Drives[driveId].Root.ItemWithPath(path).CreateUploadSession().Request().PostAsync();

                // create upload task  
                var maxChunkSize = 320 * 1024;
                var largeUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, stream, maxChunkSize);

                // upload file  
                UploadResult<DriveItem> uploadResult = largeUploadTask.UploadAsync().Result;

                if (uploadResult.UploadSucceeded)
                    return uploadResult.ItemResponse.Id;
                return string.Empty;
            }
        }

        private static async Task<string> UploadBigFile(this GraphServiceClient graphClient, string driveId, Stream fileStream, string parentItemId, string fileName)
        {
            using (Stream stream = fileStream)
            {
                var uploadSession = await graphClient.Drives[driveId].Items[parentItemId].ItemWithPath(fileName).CreateUploadSession().Request().PostAsync();

                // create upload task  
                var maxChunkSize = 320 * 1024;
                var largeUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, stream, maxChunkSize);

                // upload file  
                UploadResult<DriveItem> uploadResult = largeUploadTask.UploadAsync().Result;

                if (uploadResult.UploadSucceeded)
                    return uploadResult.ItemResponse.Id;
                return string.Empty;
            }
        }

        private static async Task<List<SharePointGraphDriveItemViewModel>> GetDriveItems(this GraphServiceClient graphClient, string driveId, List<string> folders)
        {
            folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
            var path = string.Join("/", folders);
            var result = new List<SharePointGraphDriveItemViewModel>();
            try
            {
                var driveItems = await graphClient.Drives[driveId].Root.ItemWithPath(path).Children.Request().Expand("listItem").GetAsync();
                foreach (var item in driveItems)
                {
                    if (item.Folder is null)
                    {
                        result.Add(new SharePointGraphDriveItemViewModel { Path = path, DriveItem = item });
                    }
                    else
                    {
                        var childFolders = folders.Select(f => f).ToList();
                        childFolders.Add(item.Name);
                        var items = await GetDriveItems(graphClient, driveId, childFolders);
                        result.AddRange(items.Select(d => new SharePointGraphDriveItemViewModel { Path = d.Path, DriveItem = d.DriveItem }).ToList());
                    }
                }
                return result;
            }
            catch (ServiceException ex)
            {
                return result;
            }
        }


        private static async Task<List<SharePointGraphDriveItemViewModel>> GetDriveItems(this GraphServiceClient graphClient, string driveId, string itemId, string folder)
        {
            var result = new List<SharePointGraphDriveItemViewModel>();
            try
            {
                var driveItems = await graphClient.Drives[driveId].Items[itemId].Children.Request().Expand("listItem").GetAsync();
                foreach (var item in driveItems)
                {
                    if (item.Folder is null)
                    {
                        result.Add(new SharePointGraphDriveItemViewModel { Path = folder, DriveItem = item });
                    }
                    else
                    {
                        var items = await GetDriveItems(graphClient, driveId, item.Id, item.Name);
                        result.AddRange(items.Select(d => new SharePointGraphDriveItemViewModel { Path = d.Path, DriveItem = d.DriveItem }).ToList());
                    }
                }
                return result;
            }
            catch (ServiceException ex)
            {
                return result;
            }
        }

        private static async Task<List<SharePointGraphDocPicklistViewModel>> GetDriveItemNames(this GraphServiceClient graphClient, string driveId, List<string> folders, string? filter)
        {
            folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
            var path = string.Join("/", folders);
            var result = new List<SharePointGraphDocPicklistViewModel>();

            try
            {
                //var driveItems = await graphClient.Drives[driveId].Root.ItemWithPath("Invention").Search(".png").Request().Expand("listItem").GetAsync();
                //var driveItems = await graphClient.Drives[driveId].Root.Search("q=.png").Request().Expand("listItem").GetAsync();
                var driveItems = await graphClient.Drives[driveId].Root.ItemWithPath(path).Children.Request().Expand("listItem").GetAsync();

                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.Replace("*", "").Replace("%", "");
                    filter = filter.ToLower();
                }

                foreach (var item in driveItems)
                {
                    if (item.Folder is null)
                    {
                        if (filter != null && filter.Length > 0 && !item.Name.ToLower().Contains(filter))
                            continue;

                        var fields = item.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        var isPrivate = false;
                        //var modifiedBy = item.LastModifiedBy.User != null ? item.LastModifiedBy.User.DisplayName : item.CreatedBy.User.DisplayName;
                        var modifiedBy = item.CreatedBy.User != null ? item.CreatedBy.User.AdditionalData["email"].ToString().Left(20) : "";
                        var lastModified = item.LastModifiedDateTime != null ? item.LastModifiedDateTime.Value.DateTime : item.CreatedDateTime.Value.DateTime;

                        if (fields.ContainsKey("IsPrivate"))
                            isPrivate = Convert.ToBoolean(fields.GetValueOrDefault("IsPrivate").ToString());

                        result.Add(new SharePointGraphDocPicklistViewModel
                        {
                            Folder = string.Join(SharePointSeparator.Folder, folders),
                            DocName = item.Name,
                            ModifiedBy = modifiedBy,
                            DateModified = lastModified,
                            IsPrivate = isPrivate
                        });
                    }
                    else
                    {
                        var childFolders = folders.Select(f => f).ToList();
                        childFolders.Add(item.Name);
                        var items = await GetDriveItemNames(graphClient, driveId, childFolders, filter);
                        result.AddRange(items.Select(n => n).ToList());
                    }
                }
                return result;
            }
            catch (ServiceException ex)
            {
                return result;
            }
        }

        public static async Task MarkVerified(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary, string driveItemId, string siteId, string listId)
        {
            var drive = (await graphClient.GetSiteByPath(siteRelativePath, hostname)).Drives.FirstOrDefault(d => d.Name == docLibrary);
            if (drive != null)
            {
                try
                {
                    var driveItem = await graphClient.Drives[drive.Id].Items[driveItemId].Request().Expand("listItem").GetAsync();
                    if (driveItem != null)
                    {
                        var fields = driveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        if (fields.ContainsKey("IsVerified"))
                        {
                            var isVerified = Convert.ToBoolean(fields.GetValueOrDefault("IsVerified").ToString());
                            if (!isVerified)
                            {
                                var requestBody = new FieldValueSet
                                {
                                    AdditionalData = new Dictionary<string, object>
                                        {
                                            {
                                                "IsVerified", true
                                            }
                                        }
                                };
                                var updateResult = await graphClient.Sites[siteId].Lists[listId].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                            }
                        }
                    }
                }
                catch (ServiceException)
                {
                    //return;
                }
            }
        }

        public static async Task<List<SharePointGraphTreeViewModel>> GetSiteContentTypes(this GraphServiceClient graphClient, string siteRelativePath, string hostname, string docLibrary)
        {
            var site = graphClient.GetSiteWithLists(siteRelativePath, hostname).Result;
            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
            var result = await GetSiteContentTypes(graphClient, site.Id, list.Id);
            return result;
        }

        private static async Task<List<SharePointGraphTreeViewModel>> GetSiteContentTypes(GraphServiceClient graphClient, string siteId, string listId)
        {
            var output = new List<SharePointGraphTreeViewModel>();
            var result = await graphClient.Sites[siteId].Lists[listId].ContentTypes.Request().GetAsync();
            if (result.CurrentPage.Count > 0)
            {
                var process = true;
                var items = result.CurrentPage;

                while (process)
                {
                    foreach (var item in items)
                    {
                        output.Add(new SharePointGraphTreeViewModel { Id = item.Id, Name = item.Name });
                    }

                    process = false;
                    if (result.NextPageRequest != null)
                    {
                        var page = await result.NextPageRequest.GetAsync();
                        if (page.CurrentPage.Count > 0)
                        {
                            result = page;
                            items = page.CurrentPage;
                            process = true;
                        }
                    }

                }
            }
            return output;
        }
    }
}
