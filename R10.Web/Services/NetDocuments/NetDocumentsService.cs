using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Models.NetDocumentsModels;
using System.Net;
using System.Text.Json;
using R10.Web.Extensions;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Helpers;

namespace R10.Web.Services.NetDocuments
{
    //NetDocuments API extensions for NetDocumentsClient
    public static class NetDocumentsService
    {
        public static List<string> ImageDocuments =>
            new List<string>() { "BMP", "GIF", "PNG", "JPG", "JPEG", "TIFF", "TIF" };

        public static Dictionary<string, string> DocumentIcons =>
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "DOC", "fal fa-file-word" },
                { "DOCX", "fal fa-file-word" },
                { "DOTX", "fal fa-file-word" },
                { "RTF", "fal fa-file-word" },
                { "BMP", "fal fa-image" },
                { "GIF", "fal fa-image" },
                { "PNG", "fal fa-image" },
                { "JPG", "fal fa-image" },
                { "JPEG", "fal fa-image" },
                { "TIFF", "fal fa-image" },
                { "TIF", "fal fa-image" },
                { "SVG", "fal fa-image" }, //fa-svg or fa-vector in FontAwesome 6.5
                { "XLS", "fal fa-file-excel" },
                { "XLSX", "fal fa-file-excel" },
                { "XLT", "fal fa-file-excel" },
                { "XLTX", "fal fa-file-excel" },
                { "PDF", "far fa-file-pdf" },
                { "PPT", "fal fa-file-powerpoint" },
                { "PPTX", "fal fa-file-powerpoint" },
                { "POT", "fal fa-file-powerpoint" },
                { "POTX", "fal fa-file-powerpoint" },
                { "EML", "fal fa-envelope" },
                { "MSG ", "fal fa-envelope" },
                { "ANSI", "fal fa-file-alt" },
                { "CSV", "fal fa-file-csv" },
                { "HTM", "fal fa-file-code" },
                { "HTML", "fal fa-file-code" },
                { "TTF", "fal fa-font" },
                { "ZIP ", "fal fa-file-archive" }
            };

        public static async Task<UserInfo?> GetUserInfo(this NetDocumentsClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/User/info");

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                var authUserData = JsonSerializer.Deserialize<UserInfo>(result);

                return authUserData;
            }

            return null;
        }

        public static async Task<Container> GetContainer(this NetDocumentsClient client, string? containerId)
        {
            if (!string.IsNullOrEmpty(containerId))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"v2/container/{containerId}/info"); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var containerResponse = JsonSerializer.Deserialize<ContainerResponse>(result);
                if (containerResponse != null)
                    return containerResponse;
            }

            return new Container();
        }

        /// <summary>
        /// Gets the workspace root containers
        /// </summary>
        /// <param name="client"></param>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        public static async Task<ContainersResponse?> GetContainers(this NetDocumentsClient client, string? workspaceId)
        {
            if (string.IsNullOrEmpty(workspaceId))
                return new ContainersResponse();

            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/container/{workspaceId}/summary/containers"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);
            var result = await response.GetContentAsStringAsync();
            return JsonSerializer.Deserialize<ContainersResponse>(result);
        }

        public static async Task<Folder> GetFolder(this NetDocumentsClient client, string? folderId)
        {
            if (!string.IsNullOrEmpty(folderId))
                return (await client.GetContainer(folderId)).ToFolder();

            return new Folder();
        }

        /// <summary>
        /// Gets the sub folders
        /// </summary>
        /// <param name="client"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public static async Task<FoldersResponse?> GetFolders(this NetDocumentsClient client, string? folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                return new FoldersResponse();

            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/container/{folderId}/sub?select=standardAttributes,Ancestors,&recursive=true"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            //returns all folder and subfolders
            //under a container specified by containerId
            //as flat data
            var result = await response.GetContentAsStringAsync();
            return JsonSerializer.Deserialize<FoldersResponse>(result);
        }

        public static async Task<List<Folder>> GetFolderTree(this NetDocumentsClient client, Container container)
        {
            var rootContainers = new List<Container>();
            var folderTree = new List<Folder>();
            var containerType = container.ContainerType;

            if (containerType == ContainerType.Workspace)
            {
                // get root folders
                var containersResponse = await client.GetContainers(container.Id);
                if (containersResponse?.Results != null)
                    rootContainers.AddRange(containersResponse.Results);
            }
            else if (containerType == ContainerType.Folder)
                rootContainers.Add(container);

            foreach (var rootContainer in rootContainers.OrderByDescending(c => c.ContainerType).ThenBy(c => c.Name).ToList())
            {
                var rootFolder = rootContainer.ToFolder();
                if (rootFolder != null)
                {
                    if (rootContainer.ContainerType == ContainerType.Folder)
                    {
                        var foldersResponse = await client.GetFolders(rootContainer.Id);
                        if (foldersResponse?.Results != null)
                            rootFolder.SubFolders = foldersResponse.ToFolderTree(rootContainer.Id);

                    }
                    folderTree.Add(rootFolder);
                }
            }

            return folderTree;
        }

        public static async Task<List<Folder>> GetFolderTree(this NetDocumentsClient client, string? folderId)
        {
            var foldersResponse = await client.GetFolders(folderId);
            if (foldersResponse?.Results != null)
                return foldersResponse.ToFolderTree(folderId);

            return new List<Folder>();
        }

        public static async Task<List<FolderListItem>> GetFolderList(this NetDocumentsClient client, string? containerId)
        {

            var container = await client.GetContainer(containerId);
            var folders = await client.GetFolderTree(container);

            if (folders != null && folders.Count > 0)
            {
                var folderList = folders.ToFolderList(containerId);
                folderList.Insert(0, new FolderListItem());
                return folderList;
            }

            return new List<FolderListItem>();
        }

        /// <summary>
        /// Transforms flat data returned by folders endpoint
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="parentId"></param>
        /// <returns>Heirarchical list of folders</returns>
        public static List<Folder> ToFolderTree(this FoldersResponse folders, string? parentId)
        {
            return folders.Results.ToFolderTree(parentId);
        }

        public static List<Folder> ToFolderTree(this List<Folder>? folders, string? parentId)
        {
            var folderTree = new List<Folder>();

            if (folders != null && !string.IsNullOrEmpty(parentId))
            {
                foreach (var folder in folders.Where(f => string.Equals(f.ParentId, parentId.Split('|')[0], StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.Attributes?.Name ?? "").ToList())
                {
                    folderTree.Add(folder);

                    if (folders.Any(f => f.Ancestors?.Any(a => a.Id == (folder.Id ?? "").Split('|')[0]) ?? false))
                        folder.SubFolders = folders.ToFolderTree(folder.Id);
                }
            }

            return folderTree;
        }

        /// <summary>
        /// Transforms folder tree to flat list
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="parentId"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static List<FolderListItem> ToFolderList(this List<Folder> folders, string? parentId, int level = 0)
        {
            var folderListItem = new List<FolderListItem>();

            if (folders != null && !string.IsNullOrEmpty(parentId))
            {
                foreach (var folder in folders.Where(f => f.ContainerType == ContainerType.Folder).OrderBy(f => f.Name ?? "").ToList())
                {
                    folderListItem.Add(new FolderListItem(folder.Id, folder.Name, level));

                    if (folder.SubFolders != null && (folder.HasSubFolders ?? false))
                        folderListItem.AddRange(folder.SubFolders.ToFolderList(folder.Id, level + 1));
                }
            }

            return folderListItem;
        }

        public static async Task<string> GetContentAsStringAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            //todo: error handling
            //throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);
            return "{}";
        }

        public static bool IsImage(this Document document) => ImageDocuments.Any(s => string.Equals(s, document.Attributes?.Ext, StringComparison.OrdinalIgnoreCase));

        public static string GetIcon(this Document document) => DocumentIcons.Where(d =>
            //string.Equals(d.Key, document.Type, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(d.Key, document.Attributes?.Ext, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;

        public static string GetFileName(this Document document) => string.Concat(document.Attributes?.Name, ".", document.Attributes?.Ext?.ToLower());

        public static string GetContainerId(this List<QueryFilterViewModel> mainSearchFilters)
        {
            var containerId = "";
            var containerIdProperty = mainSearchFilters.FirstOrDefault(f => f.Property == "ContainerId");

            if (containerIdProperty != null)
            {
                containerId = containerIdProperty.Value;
                mainSearchFilters.Remove(containerIdProperty);
            }

            return containerId;
        }

        /// <summary>
        /// Get documents in a container
        /// GET /v2/container/{{container_envid}}/search?select=StandardAttributes,Ancestors&q=not =11(msg OR eml OR ndsq OR ndfld OR ndcs)&orderby=3|asc
        /// </summary>
        /// <param name="client"></param>
        /// <param name="criteria"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async Task<List<Document>> GetDocuments(this NetDocumentsClient client, List<QueryFilterViewModel>? criteria,
            int skip = 0, int top = 500)
        {
            //paging params:
            //skip (integer)
            //  Default:0
            //  Specifies the position of the first item to be returned from the result set.
            //top (integer)
            //  Default:100, Min 1 ┃ Max 500
            //  Specifies the maximum number of documents to include in the response.

            var containerId = criteria?.GetContainerId();
            if (!string.IsNullOrEmpty(containerId))
            {
                var requestUri = $"v2/container/{containerId}/search?select=StandardAttributes,Ancestors" +
                    $"&skip={skip}&top={top}";
                var query = "&q=";
                var includeSubtree = false;

                if (criteria != null && criteria.Any())
                {
                    var name = criteria.FirstOrDefault(f => f.Property == "Name");
                    if (name != null)
                    {
                        query += $"=3({name.Value})";
                        criteria.Remove(name);
                    }

                    DateTime? dateFrom = null;
                    DateTime? dateTo = null;
                    var editDateFrom = criteria.FirstOrDefault(f => f.Property == "EditDateFrom");
                    if (editDateFrom != null)
                    {
                        dateFrom = DateTime.Parse(editDateFrom.Value);
                        criteria.Remove(editDateFrom);
                    }

                    var editDateTo = criteria.FirstOrDefault(f => f.Property == "EditDateTo");
                    if (editDateTo != null)
                    {
                        dateTo = DateTime.Parse(editDateTo.Value);
                        criteria.Remove(editDateTo);
                    }

                    if (dateFrom != null && dateTo == null)
                        dateTo = DateTime.Now.AddDays(1); //MaxValue does not work

                    if (dateFrom == null && dateTo != null)
                        dateFrom = DateTime.MinValue;

                    if (dateFrom != null && dateTo != null)
                    {
                        query += $"=7(^{((DateTime)dateFrom).ToOADate()}-{((DateTime)dateTo).ToOADate()})";
                    }

                    var type = criteria.FirstOrDefault(f => f.Property == "Type");
                    if (type != null)
                    {
                        var types = type.GetValueList();
                        if (types.Count > 0)
                            query += $"=11({string.Join(" OR ", types)})";
                        else
                            query += $"=11({type.Value})";

                        criteria.Remove(type);
                    }

                    //not used
                    //netdocs search always include sub folders
                    var includeSubFolders = criteria.FirstOrDefault(f => f.Property == "IncludeSubFolders");
                    if (includeSubFolders != null)
                    {
                        includeSubtree = true;
                        criteria.Remove(includeSubFolders);
                    }
                }

                if (!query.Contains("=11("))
                    //query += "not =11(msg OR eml OR ndsq OR ndfld OR ndcs)"; //documents only
                    query += "not =11(ndsq OR ndfld OR ndcs)"; //documents and emails only

                return await client.GetDocuments(requestUri + query);
            }

            return new List<Document>();
        }

        public static async Task<DocumentsResponse?> GetDocumentsResponse(this NetDocumentsClient client, string requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            var result = await response.GetContentAsStringAsync();
            return JsonSerializer.Deserialize<DocumentsResponse>(result);
        }

        public static async Task<List<Document>> GetDocuments(this NetDocumentsClient client, string requestUri)
        {
           var documents = new List<Document>();
            var documentsResponse = await client.GetDocumentsResponse(requestUri);
            var skipToken = documentsResponse?.SkipToken;

            if (documentsResponse?.Results != null)
            {
                documents.AddRange(documentsResponse.Results);

                while (skipToken != null)
                {
                    documentsResponse = await client.GetDocumentsResponse(requestUri + $"&skipToken={skipToken}");
                    skipToken = documentsResponse?.SkipToken;
                    if (documentsResponse?.Results != null)
                        documents.AddRange(documentsResponse.Results);
                }
            }

            return documents;
        }

        public static async Task<List<Document>> GetDocuments(this NetDocumentsClient client, string containerId, List<QueryFilterViewModel>? criteria,
            int skip = 0, int top = 500)
        {
            if (criteria == null)
                criteria = new List<QueryFilterViewModel>();

            criteria.Add(new QueryFilterViewModel() { Property = "ContainerId", Value = containerId });
            return await client.GetDocuments(criteria, skip, top);
        }

        public static async Task<List<Document>> GetDocuments(this NetDocumentsClient client, string containerId, bool includeSubFolders = false,
            int offset = 0, int limit = 1000)
        {
            var criteria = new List<QueryFilterViewModel>()
            {
                new QueryFilterViewModel() { Property = "ContainerId", Value = containerId  },
                new QueryFilterViewModel() { Property = "IncludeSubFolders", Value = includeSubFolders ? "true" : "false" }
            };
            return await client.GetDocuments(criteria, offset, limit);
        }

        public static async Task<HttpResponseMessage> GetDocument(this NetDocumentsClient client, string docId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"v1/document/{docId}"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);

            return response;
        }

        public static async Task<MemoryStream?> GetDocumentAsStream(this NetDocumentsClient client, string docId)
        {
            var response = await client.GetDocument(docId);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return new MemoryStream(bytes);
        }

        public static async Task<FileContentResult> DownloadDocument(this NetDocumentsClient client,
            string docId)
        {
            var response = await client.GetDocument(docId);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName;
            var contentType = response.Content.Headers.ContentType?.MediaType;

            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.ReplaceInvalidFilenameChars();
                contentType = ImageHelper.GetContentType(fileName);
            }

            var fileContent = new FileContentResult(bytes, contentType ?? "application/octet-stream");

            if (!string.IsNullOrEmpty(fileName))
                fileContent.FileDownloadName = fileName;

            return fileContent;
        }

        public static async Task<Document?> UploadDocument(this NetDocumentsClient client, string folderId, ByteArrayContent bytes, string fileName)
        {
            var requestUri = $"v1/document";
            var content = new MultipartFormDataContent();

            content.Add(new StringContent("upload"), "action");
            content.Add(new StringContent(folderId), "destination");
            content.Add(new StringContent(Path.GetFileNameWithoutExtension(fileName)), "name");
            content.Add(new StringContent(Path.GetExtension(fileName).TrimStart('.')), "extension");
            content.Add(new StringContent("true"), "failOnError");
            content.Add(bytes, "file", fileName);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);

            // returns v1 document profile
            var result = await response.GetContentAsStringAsync();
            var documentResponse = JsonSerializer.Deserialize<DocumentUploadResponse>(result);

            // get v2 document info
            if (documentResponse != null)
                return await client.GetDocumentProfile(documentResponse.DocId);

            return null;
        }

        public static async Task<Document?> CreateVersion(this NetDocumentsClient client, string docId, ByteArrayContent bytes, string fileName)
        {
            var requestUri = $"v1/document/{docId}/new?official=Y";
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(Path.GetFileNameWithoutExtension(fileName)), "name");
            content.Add(new StringContent(Path.GetExtension(fileName).TrimStart('.')), "extension");
            content.Add(new StringContent("true"), "failOnError");
            content.Add(bytes, "file", fileName);

            var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content };
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);

            // returns v1 document profile
            var result = await response.GetContentAsStringAsync();
            var documentResponse = JsonSerializer.Deserialize<DocumentUploadResponse>(result);

            // get v2 document info
            if (documentResponse != null)
                return await client.GetDocumentProfile(documentResponse.DocId);

            return null;
        }

        public static async Task<Document?> GetDocumentProfile(this NetDocumentsClient client, string? docId)
        {
            if (string.IsNullOrEmpty(docId))
                return null;

            var request = new HttpRequestMessage(HttpMethod.Get, $"v2/document/{docId}/info?select=standardAttributes,customAttributes,Ancestors"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            var result = await response.GetContentAsStringAsync();
            return JsonSerializer.Deserialize<Document>(result);
        }

        public static async Task UpdateDocumentProfile(this NetDocumentsClient client, string docId, UpdatableDocumentProfile documentProfile)
        {
            if (!string.IsNullOrEmpty(docId))
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"v1/document/{docId}/info") //no leading slash to use BaseAddress
                {
                    Content = new StringContent(JsonSerializer.Serialize(documentProfile), Encoding.UTF8, "application/json")
                };
                await client.SendAsync(request); //returns 204 No Content
            }
        }

        public static async Task<bool> MoveDocument(this NetDocumentsClient client, string? docId, string? folderId, string? destinationFolderId)
        {
            if (string.IsNullOrEmpty(docId))
                return false;

            //var library = client.GetLibrary(docId);
            //var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/folders/{folderId}/documents/{docId}/move") //no leading slash to use BaseAddress
            //{
            //    Content = new StringContent(JsonSerializer.Serialize(new { destination_folder_id = destinationFolderId }), Encoding.UTF8, "application/json")
            //};
            //var response = await client.SendAsync(request);
            //return response.IsSuccessStatusCode;
            throw new NotImplementedException();
        }

        public static async Task<Container> GetWorkspaceByMatter(this NetDocumentsClient httpClient, string? attributeKey, string? attributeParent = "")
        {
            var workspaceId = await httpClient.GetWorkspaceId(attributeKey, attributeParent);

            if (!string.IsNullOrEmpty(workspaceId))
                return await httpClient.GetContainer(workspaceId);

            return new Container();
        }

        public static async Task<string?> GetWorkspaceId(this NetDocumentsClient client, string? attributeKey, string? attributeParent = "")
        {
            //{{base_url}}/v1/Workspace/{{Default_Cabinet}}/CPI/0001/info
            var requestUrl = $"v1/workspace/{client.Cabinet}";
            if (!string.IsNullOrEmpty(attributeParent))
                requestUrl += $"/{attributeParent}";

            requestUrl += $"/{attributeKey}/info";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            var result = await response.GetContentAsStringAsync();
            var workspaceResponse = JsonSerializer.Deserialize<WorkspaceResponse>(result);

            return workspaceResponse?.StandardAttributes?.EnvId;
        }

        public static async Task<ProfileAttribute?> GetProfileAttribute(this NetDocumentsClient client, int attributeId, string? attributeKey, string? attributeParent = "")
        {
            if (attributeId == 0 || string.IsNullOrEmpty(attributeKey))
                return new ProfileAttribute();

            var requestUrl = $"v1/attributes/{client.Repository}/{attributeId}";
            if (!string.IsNullOrEmpty(attributeParent))
                requestUrl += $"/{attributeParent}";

            requestUrl += $"/{attributeKey}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            var result = await response.GetContentAsStringAsync();
            return JsonSerializer.Deserialize<ProfileAttribute>(result);
        }

        public static async Task SetProfileAttribute(this NetDocumentsClient client, int attributeId, ProfileAttribute profileAttribute)
        {
            var requestUrl = $"v1/attributes/{client.Repository}/{attributeId}";
            if (!string.IsNullOrEmpty(profileAttribute.Parent))
                requestUrl += $"/{profileAttribute.Parent}";

            requestUrl += $"/{profileAttribute.Key}";

            var request = new HttpRequestMessage(HttpMethod.Put, requestUrl) //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(profileAttribute), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);
        }

        public static async Task DeleteDocument(this NetDocumentsClient client, string docId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"v1/document/{docId}"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);
        }

        public static async Task DeleteFolder(this NetDocumentsClient client, string folderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"v1/folder/{folderId}"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);
        }

        public static async Task RenameFolder(this NetDocumentsClient client, string folderId, string folderName)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"v1/folder/{folderId}/info") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new UpdatableDocumentProfile()
                {
                    StandardAttributes = new UpdatableStandardAttributes() { Name = folderName }
                }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);
        }

        public static async Task<Folder?> CreateFolder(this NetDocumentsClient client, string parentFolderId, string name)
        {
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(name), "name");
            content.Add(new StringContent(parentFolderId), "parent");

            var request = new HttpRequestMessage(HttpMethod.Post, $"v1/folder") //no leading slash to use BaseAddress
            {
                Content = content
            };
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);

            var result = await response.GetContentAsStringAsync();
            var folderCreateResponse = JsonSerializer.Deserialize<FolderCreateResponse>(result);

            // get v2 folder info
            if (folderCreateResponse != null)
                return (await client.GetContainer(folderCreateResponse?.StandardAttributes?.EnvId)).ToFolder();

            return null;
        }

        public static async Task MoveFolder(this NetDocumentsClient client, string folderId, string newParentId)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"v1/folder/{folderId}/parent") //no leading slash to use BaseAddress
            {
                Content = new StringContent($"{{ \"envId\": \"{newParentId}\" }}", Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new NetDocumentsServiceException(await response.GetErrorMessage(), response.StatusCode);
        }
    }
}
