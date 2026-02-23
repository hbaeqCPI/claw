using Microsoft.AspNetCore.Mvc;
using Microsoft.Rest;
using R10.Web.Models.IManageModels;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Headers;
using R10.Core.Helpers;
using R10.Web.Areas.Shared.ViewModels;
using System.Net.Http;
using System.Net.Http.Headers;
using R10.Web.Helpers;
using R10.Web.Extensions;
using System.Text.Json.Serialization;

namespace R10.Web.Services.iManage
{
    //iManage Work API extensions for iManageClient
    public static class iManageWorkService
    {
        public static List<string> ImageDocuments =>
            new List<string>() { "BMP", "GIF", "PNG", "JPG", "JPEG", "TIFF" };

        public static Dictionary<string, string> DocumentIcons =>
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "ACROBAT", "far fa-file-pdf" },
                { "DOC", "fal fa-file-word" },
                { "DOCX", "fal fa-file-word" },
                { "WORDX", "fal fa-file-word" },
                { "BMP", "fal fa-image" },
                { "GIF", "fal fa-image" },
                { "PNG", "fal fa-image" },
                { "JPG", "fal fa-image" },
                { "JPEG", "fal fa-image" },
                { "TIFF", "fal fa-image" },
                { "SVG", "fal fa-image" }, //fa-svg or fa-vector in FontAwesome 6.5
                { "EXCEL", "fal fa-file-excel" },
                { "EXCELX", "fal fa-file-excel" },
                { "XLS", "fal fa-file-excel" },
                { "XLSX", "fal fa-file-excel" },
                { "PPT", "fal fa-file-powerpoint" },
                { "PPTX", "fal fa-file-powerpoint" },
                { "EML", "fal fa-envelope" },
                { "MIME ", "fal fa-envelope" },
                { "ANSI", "fal fa-file-alt" },
                { "CSV", "fal fa-file-csv" },
                { "HTM", "fal fa-file-code" },
                { "HTML", "fal fa-file-code" },
                { "TTF", "fal fa-font" },
                { "ZIP ", "fal fa-file-archive" }
            };

        /// <summary>
        /// Scoped search for documents
        /// POST /work/api/v2/customers/{customerId}/libraries/{libraryId}/documents/search
        /// Returns all the documents that match the search criteria, with only the specified document profile fields in the response.
        /// By default, the response includes id, wstype, and iwl.
        /// The request body parameter filters specifies the search criteria.
        /// The profile_fields parameter specifies the set of fields to return in the response object.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="criteria"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async Task<List<Document>> SearchDocuments(this iManageClient client, List<QueryFilterViewModel>? criteria,
            int limit = 1000)
        {
            var containerId = criteria?.GetContainerId();
            if (!string.IsNullOrEmpty(containerId))
            {
                var library = client.GetLibrary(containerId);
                var requestUri = $"libraries/{library}/documents/search";
                var includeSubtree = false;
                var name = "";
                var editDateFrom = "";
                var editDateTo = "";
                var docType = "";

                if (criteria != null && criteria.Any())
                {
                    var queryFilter = criteria.FirstOrDefault(f => f.Property == "Name");
                    if (queryFilter != null)
                    {
                        name = queryFilter.Value;
                        criteria.Remove(queryFilter);
                    }

                    queryFilter = criteria.FirstOrDefault(f => f.Property == "EditDateFrom");
                    if (queryFilter != null)
                    {
                        editDateFrom = $"{queryFilter.Value}Z"; //ISO 8601 (2024-02-21T00:00:00Z)
                        criteria.Remove(queryFilter);
                    }

                    queryFilter = criteria.FirstOrDefault(f => f.Property == "EditDateTo");
                    if (queryFilter != null)
                    {
                        editDateTo = $"{queryFilter.Value}Z"; //ISO 8601 (2024-02-21T00:00:00Z)
                        criteria.Remove(queryFilter);
                    }

                    queryFilter = criteria.FirstOrDefault(f => f.Property == "Type");
                    if (queryFilter != null)
                    {
                        var types = queryFilter.GetValueList();
                        if (types.Count > 0)
                            docType = string.Join(",", types);
                        else
                            docType = queryFilter.Value;

                        criteria.Remove(queryFilter);
                    }

                    //include sub folders defaults to true if not passed
                    queryFilter = criteria.FirstOrDefault(f => f.Property == "IncludeSubFolders");
                    if (queryFilter != null)
                    {
                        includeSubtree = true;
                        criteria.Remove(queryFilter);
                    }
                }

                var profileFields = new
                {
                    document = new string[] 
                    { 
                        "id", "author", "name", "extension", "type", "size", "is_checked_out", "is_in_use",
                        "in_use_by", "create_date", "edit_date", "last_user", "version", "workspace_id"
                    }
                };

                var filters = new
                {
                    container_id = containerId,
                    //name = name,
                    //edit_date_from = editDateFrom,
                    //edit_date_to = editDateTo,
                    //type = docType,
                    include_subtree = includeSubtree
                };

                var request = new HttpRequestMessage(HttpMethod.Post, requestUri) //no leading slash to use BaseAddress
                {
                    Content = new StringContent(JsonSerializer.Serialize(new { profile_fields = profileFields, filters = filters }), Encoding.UTF8, "application/json")
                };

                //var request = new HttpRequestMessage(HttpMethod.Post, requestUri); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var searchResponse = JsonSerializer.Deserialize<DocumentSearchResponse>(result);

                if (searchResponse?.Data != null)
                {
                    return searchResponse.Data;
                }
            }

            return new List<Document>();
        }

        /// <summary>
        /// Get library documents
        /// GET /work/api/v2/customers/{customerId}/libraries/{libraryId}/ documents
        /// Gets all documents that match the specified search criteria.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="criteria"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async Task<List<Document>> GetDocuments(this iManageClient client, List<QueryFilterViewModel>? criteria,
            int offset = 0, int limit = 1000)
        {
            //paging params:
            //limit (integer)
            //  Default:500, Min 1 ┃ Max 9999
            //  Specifies the maximum number of documents to include in the response.
            //offset (integer)
            //  Default:0
            //  Specifies the position of the first item to be returned from the result set.
            //paging_mode (enum)
            //  Allowed : standard ┃ standard_cursor
            //  Specifies the paging mode to be used to retrieve the result set.
            //      NOTE: The following exceptions to the above occur when the paging_mode is specified as standard_cursor:
            //            The filters total, anywhere and comments are ignored.
            //            The response fields total_count, from, to, cc, and bcc are not returned.
            //total (boolean)
            //  Specifies to include the total count of items found in the response.

            var containerId = criteria?.GetContainerId();
            if (!string.IsNullOrEmpty(containerId))
            {
                var library = client.GetLibrary(containerId);
                var requestUri = $"libraries/{library}/documents?container_id={containerId}" +
                    $"&paging_mode=standard&total=true" +
                    $"&offset={offset}&limit={limit}";
                var includeSubtree = false;

                if (criteria != null && criteria.Any())
                {
                    var name = criteria.FirstOrDefault(f => f.Property == "Name");
                    if (name != null)
                    {
                        requestUri += $"&name={name.Value}";
                        criteria.Remove(name);
                    }

                    var editDateFrom = criteria.FirstOrDefault(f => f.Property == "EditDateFrom");
                    if (editDateFrom != null)
                    {
                        requestUri += $"&edit_date_from={editDateFrom.Value}Z"; //ISO 8601 (2024-02-21T00:00:00Z)
                        criteria.Remove(editDateFrom);
                    }

                    var editDateTo = criteria.FirstOrDefault(f => f.Property == "EditDateTo");
                    if (editDateTo != null)
                    {
                        requestUri += $"&edit_date_to={editDateTo.Value}Z"; //ISO 8601 (2024-02-21T00:00:00Z)
                        criteria.Remove(editDateTo);
                    }

                    var type = criteria.FirstOrDefault(f => f.Property == "Type");
                    if (type != null)
                    {
                        var types = type.GetValueList();
                        if (types.Count > 0)
                            requestUri += $"&type={string.Join(",", types)}";
                        else
                            requestUri += $"&type={type.Value}";

                        criteria.Remove(type);
                    }

                    //include sub folders defaults to true if not passed
                    var includeSubFolders = criteria.FirstOrDefault(f => f.Property == "IncludeSubFolders");
                    if (includeSubFolders != null)
                    {
                        includeSubtree = true;
                        criteria.Remove(includeSubFolders);
                    }
                }
                requestUri += $"&include_subtree={includeSubtree.ToString().ToLower()}";

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var documentsResponse = JsonSerializer.Deserialize<DocumentsResponse>(result);

                if (documentsResponse?.Data?.Results != null)
                {
                    return documentsResponse.Data.Results;
                }
            }

            return new List<Document>();
        }

        public static async Task<List<Document>> GetDocuments(this iManageClient client, string containerId, bool includeSubFolders = false,
            int offset = 0, int limit = 1000)
        {
            var criteria = new List<QueryFilterViewModel>()
            {
                new QueryFilterViewModel() { Property = "ContainerId", Value = containerId  },
                new QueryFilterViewModel() { Property = "IncludeSubFolders", Value = includeSubFolders ? "true" : "false" }
            };
            return await client.GetDocuments(criteria, offset, limit);
        }

        public static async Task<List<Document>> GetDocuments(this iManageClient client, string containerId, List<QueryFilterViewModel>? criteria,
            int offset = 0, int limit = 1000)
        {
            if (criteria == null)
                criteria = new List<QueryFilterViewModel>();

            criteria.Add(new QueryFilterViewModel() { Property = "ContainerId", Value = containerId });
            return await client.GetDocuments(criteria, offset, limit);
        }

        public static async Task<Document> GetDocumentProfile(this iManageClient client, string? docId)
        {
            if (!string.IsNullOrEmpty(docId))
            {
                var library = client.GetLibrary(docId);
                var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/documents/{docId}"); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var documentResponse = JsonSerializer.Deserialize<DocumentResponse>(result);
                if (documentResponse?.Data != null)
                {
                    return documentResponse.Data;
                }
            }

            return new Document();
        }

        public static async Task<Document> UpdateDocumentProfile(this iManageClient client, string docId, UpdatableDocumentProfile documentProfile)
        {
            if (!string.IsNullOrEmpty(docId))
            {
                var library = client.GetLibrary(docId);
                var request = new HttpRequestMessage(HttpMethod.Patch, $"libraries/{library}/documents/{docId}") //no leading slash to use BaseAddress
                {
                    Content = new StringContent(JsonSerializer.Serialize(documentProfile), Encoding.UTF8, "application/json")
                };
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var documentResponse = JsonSerializer.Deserialize<DocumentResponse>(result);
                if (documentResponse?.Data != null)
                {
                    return documentResponse.Data;
                }
            }

            return new Document();
        }

        public static async Task<List<ParentFolder>> GetParentFolders(this iManageClient client, string? containerId, int[] documentNumbers)
        {
            if (!string.IsNullOrEmpty(containerId))
            {
                var limit = 50;
                var library = client.GetLibrary(containerId);
                var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/documents/parents")
                {
                    Content = new StringContent(JsonSerializer.Serialize(new { document_numbers = documentNumbers, limit = limit }), Encoding.UTF8, "application/json")
                };
                var response = await client.SendAsync(request);
                var result = await response.GetContentAsStringAsync();
                var parentFoldersResponse = JsonSerializer.Deserialize<ParentFoldersResponse>(result);
                if (parentFoldersResponse?.Data != null)
                {
                    return parentFoldersResponse.Data;
                }
            }

            return new List<ParentFolder>();
        }

        public static async Task<List<FolderPath>> GetFolderPaths(this iManageClient client, string? documentId)
        {
            if (!string.IsNullOrEmpty(documentId))
            {
                var library = client.GetLibrary(documentId);
                var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/documents/{documentId}/path"); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);
                var result = await response.GetContentAsStringAsync();
                var resultResponse = System.Text.Json.JsonSerializer.Deserialize<FolderPathsResponse>(result);
                if (resultResponse != null && resultResponse.Data != null && resultResponse.Data.Count > 0)
                {
                    var resultData = resultResponse.Data.FirstOrDefault();
                    if (resultData != null && resultData.Count > 0)
                    {
                        return resultData;
                    }
                }
            }

            return new List<FolderPath>();
        }

        public static async Task<Container> GetContainer(this iManageClient client, string? containerId)
        {
            if (!string.IsNullOrEmpty(containerId))
            {
                var library = client.GetLibrary(containerId);
                var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/containers/{containerId}"); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var containerResponse = JsonSerializer.Deserialize<ContainerResponse>(result);
                if (containerResponse?.Data != null)
                {
                    return containerResponse.Data;
                }
            }

            return new Container();
        }

        public static async Task<FoldersResponse?> GetFolders(this iManageClient client, string? containerId)
        {
            if (string.IsNullOrEmpty(containerId))
                return new FoldersResponse();

            var limit = 50;
            var library = client.GetLibrary(containerId);
            var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/folders?container_id={containerId}&limit={limit}"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            //returns all folder and subfolders
            //under a container specified by containerId
            //as flat data
            var result = await response.GetContentAsStringAsync();
            return JsonSerializer.Deserialize<FoldersResponse>(result);
        }

        /// <summary>
        /// Get all folders in a container and transforms the response into a folder tee
        /// </summary>
        /// <param name="client"></param>
        /// <param name="containerId"></param>
        /// <returns>List of folders in hierarchical format</returns>
        public static async Task<List<Folder>> GetFolderTree(this iManageClient client, string? containerId)
        {
            var foldersResponse = await client.GetFolders(containerId);
            if (foldersResponse?.Data != null)
                return foldersResponse.ToFolderTree(containerId);

            return new List<Folder>();
        }

        /// <summary>
        /// Transforms flat data returned by folders endpoint
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="parentId"></param>
        /// <returns>Heirarchical list of folders</returns>
        public static List<Folder> ToFolderTree(this FoldersResponse folders, string? parentId)
        {
            var folderTree = new List<Folder>();

            if (folders?.Data != null && !string.IsNullOrEmpty(parentId))
            {
                foreach (var folder in folders.Data.Where(f => string.Equals(f.ParentId, parentId, StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.Name ?? "").ToList())
                {
                    folderTree.Add(folder);

                    if (folder.HasSubFolders ?? false)
                        folder.SubFolders = folders.ToFolderTree(folder.Id);
                }
            }

            return folderTree;
        }

        public static List<FolderListItem> ToFolderList(this List<Folder> folders, string? parentId, int level = 0)
        {
            var folderListItem = new List<FolderListItem>();

            if (folders != null && !string.IsNullOrEmpty(parentId))
            {
                foreach (var folder in folders.Where(f => f.ContainerType == ContainerType.Folder && string.Equals(f.ParentId, parentId, StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.Name ?? "").ToList())
                {
                    folderListItem.Add(new FolderListItem(folder.Id, folder.Name, level));

                    if (folder.HasSubFolders ?? false)
                        folderListItem.AddRange(folders.ToFolderList(folder.Id, level + 1));
                }
            }

            return folderListItem;
        }

        public static async Task<Document?> UploadDocument(this iManageClient client, ByteArrayContent bytes, string fileName, string url)
        {
            var content = new MultipartFormDataContent();
            var profile = JsonSerializer.Serialize(new
            {
                warnings_for_required_and_disabled_fields = true,
                doc_profile = new
                {
                    name = Path.GetFileNameWithoutExtension(fileName),
                    extension = Path.GetExtension(fileName).TrimStart('.'),
                    size = bytes.Headers.ContentLength
                }
            });
            var stringContent = new StringContent(profile, Encoding.UTF8);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            content.Add(stringContent, "profile");
            content.Add(bytes, "file", fileName);

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new iManageServiceException(await response.GetErrorMessage(), response.StatusCode);

            var result = await response.GetContentAsStringAsync();
            var documentResponse = JsonSerializer.Deserialize<DocumentResponse>(result);

            return documentResponse?.Data;
        }

        public static async Task<Document?> UploadDocument(this iManageClient client, string folderId, ByteArrayContent bytes, string fileName)
        {
            return await client.UploadDocument(bytes, fileName, $"libraries/{client.GetLibrary(folderId)}/folders/{folderId}/documents");
        }

        public static async Task<Document?> CreateVersion(this iManageClient client, string docId, ByteArrayContent bytes, string fileName)
        {
            return await client.UploadDocument(bytes, fileName, $"libraries/{client.GetLibrary(docId)}/documents/{docId}/versions");
        }

        public static async Task<bool> MoveFolder(this iManageClient client, string folderId, string destinationFolderId)
        {
            if (string.IsNullOrEmpty(folderId) || string.IsNullOrEmpty(destinationFolderId))
                return false;

            var library = client.GetLibrary(folderId);
            var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/folders/{folderId}/move") //no leading slash to use BaseAddress
            { 
                Content = new StringContent(JsonSerializer.Serialize(new { destination_id = destinationFolderId }), Encoding.UTF8, "application/json")
            }; 
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public static async Task<Workspace?> CreateWorkspace(this iManageClient client, string name)
        {
            var library = client.GetLibrary();
            var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/workspaces") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new { name = name, default_security = "public" }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new iManageServiceException(await response.GetErrorMessage(), response.StatusCode);

            var result = await response.GetContentAsStringAsync();
            var workspaceResponse = JsonSerializer.Deserialize<WorkspaceResponse>(result);
            var workspace = workspaceResponse?.Data;
            
            return workspace;
        }

        /// <summary>
        /// Updates the name of the specified workspace.
        /// The minimum access permission required on the library to implement this request: NRTADMIN.
        /// https://help.imanage.com/hc/en-us/articles/4412558535067-iManage-Work-Universal-API-Reference-Guide-REST-v2#patch-/work/api/v2/customers/-customerId-/libraries/-libraryId-/workspaces/-workspaceId-
        /// </summary>
        /// <param name="client"></param>
        /// <param name="workspaceId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="iManageServiceException"></exception>
        public static async Task<bool> RenameWorkspace(this iManageClient client, string workspaceId, string name)
        {
            var library = client.GetLibrary();
            var request = new HttpRequestMessage(HttpMethod.Patch, $"libraries/{library}/workspaces/{workspaceId}") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new { name = name }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Create workspace folder structure based on a template
        /// https://help.imanage.com/hc/en-us/articles/18889715383579-Use-Universal-API-to-create-a-workspace-from-an-iManage-Work-template
        /// </summary>
        /// <param name="client"></param>
        /// <param name="workspaceId"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public static async Task ApplyWorkspaceTemplate(this iManageClient client, string workspaceId, string templateId)
        {
            var library = client.GetLibrary();

            //get template profile
            var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/workspaces/{templateId}/name-value-pairs"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);
            var result = await response.GetContentAsStringAsync();
            var templateProfileResponse = JsonSerializer.Deserialize<NameValuePairsResponse>(result);
            var templateProfile = new Dictionary<string, string>();

            if (templateProfileResponse?.Data != null)
                templateProfile = templateProfileResponse.Data;

            templateProfile.Add("TemplateId", templateId);

            //get template folders
            var templateFolders = await client.GetFolderTree(templateId);

            //apply security
            //https://help.imanage.com/hc/en-us/articles/4412558535067-iManage-Work-Universal-API-Reference-Guide-REST-v2#post-/work/api/v2/customers/-customerId-/libraries/-libraryId-/workspaces/-workspaceId-/security

            //apply template profile
            request = new HttpRequestMessage(HttpMethod.Patch, $"libraries/{library}/workspaces/{workspaceId}/name-value-pairs") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(templateProfile), Encoding.UTF8, "application/json")
            };
            await client.SendAsync(request);

            //create folders
            foreach(var folder in templateFolders )
            {
                await client.CreateFolderTree(workspaceId, ContainerType.Workspace, folder);
            }
        }

        /// <summary>
        /// Create folder structure based on workspace template
        /// </summary>
        /// <param name="folderTree"></param>
        /// <returns></returns>
        public static async Task CreateFolderTree(this iManageClient client, string parentId, ContainerType parentContainerType, Folder folderTree)
        {
            if (!string.IsNullOrEmpty(parentId) && !string.IsNullOrEmpty(folderTree.Name))
            {
                Folder? newFolder = null;

                if (folderTree.FolderType == FolderType.Regular)
                {
                    var folder = await client.GetFolder(folderTree.Id);
                    newFolder = await client.CreateFolder(parentId, parentContainerType, folder);
                }
                else if (folderTree.FolderType == FolderType.Search)
                {
                    var searchFolder = await client.GetSearchFolder(folderTree.Id);
                    if (searchFolder?.SearchProfile != null)
                        newFolder = await client.CreateSearchFolder(parentId, parentContainerType, searchFolder);
                }
                else if (folderTree.FolderType == FolderType.Tab)
                {
                    newFolder = await client.CreateWorkspaceTab(parentId, folderTree);
                }

                //create sub folders
                if (newFolder != null && !string.IsNullOrEmpty(newFolder.Id) && folderTree.SubFolders != null)
                {
                    foreach (var folder in folderTree.SubFolders)
                    {
                        await client.CreateFolderTree(newFolder.Id, ContainerType.Folder, folder);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new folder at the root level of the specified workspace.
        /// 
        /// Attempting to use this for any other location results in an error. 
        /// If the folder is to be created within another folder, 
        /// instead use see POST /customers/{customerId}/libraries/{libraryId}/folders/{folderId}/ subfolders
        /// </summary>
        /// <param name="client"></param>
        /// <param name="workspaceId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<Folder?> CreateRootFolder(this iManageClient client, string workspaceId, string name)
        {
            var library = client.GetLibrary(workspaceId);
            var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/workspaces/{workspaceId}/folders") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new { name = name }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            var result = await response.GetContentAsStringAsync();
            var folderResponse = JsonSerializer.Deserialize<FolderResponse>(result);
            if (folderResponse?.Data != null)
                return folderResponse.Data;

            return null;
        }

        /// <summary>
        /// Creates a folder within another folder known as subfolder.
        /// 
        /// The minimum access permission required to a folder to add a subfolder is read_write.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="folderId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<Folder?> CreateSubFolder(this iManageClient client, string folderId, string name)
        {
            var library = client.GetLibrary(folderId);
            var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/folders/{folderId}/subfolders") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new { name = name }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            var result = await response.GetContentAsStringAsync();
            var folderResponse = JsonSerializer.Deserialize<FolderResponse>(result);
            if (folderResponse?.Data != null)
                return folderResponse.Data;

            return null;
        }

        public static async Task<bool> RenameFolder(this iManageClient client, string folderId, string name)
        {
            if (string.IsNullOrEmpty(folderId) || string.IsNullOrEmpty(name))
                return false;

            var library = client.GetLibrary(folderId);
            var request = new HttpRequestMessage(HttpMethod.Patch, $"libraries/{library}/folders/{folderId}") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new { name = name }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes the specified folder. A successful delete folder request does not return a response object.
        /// 
        /// A delete request fails if the following conditions are not satisfied:
        /// 
        /// The folder must not contain any child items.Folders being deleted independently or as part of a workspace deletion, cannot contain any subfolders or documents. Check for the folder children using the request:
        /// GET /customers/{customerId}/libraries/{libraryId}/folders/{folderId}/children
        /// 
        /// The user must have the minimum of read/write access permission to that item and to all other attached items. This applies to all create, delete, and modify operations and is not unique to this operation. Check access permission for each library with the operations call:
        /// GET /customers/{ customerId}/ libraries /{ libraryId}/ operations
        /// </summary>
        /// <param name="client"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteFolder(this iManageClient client, string folderId)
        {
            if (string.IsNullOrEmpty(folderId))
                return false;

            var library = client.GetLibrary(folderId);
            var request = new HttpRequestMessage(HttpMethod.Delete, $"libraries/{library}/folders/{folderId}"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> DeleteDocument(this iManageClient client, string docId)
        {
            if (string.IsNullOrEmpty(docId))
                return false;

            var library = client.GetLibrary(docId);
            var request = new HttpRequestMessage(HttpMethod.Delete, $"libraries/{library}/documents/{docId}"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> MoveDocument(this iManageClient client, string docId, string folderId, string destinationFolderId)
        {
            if (string.IsNullOrEmpty(docId))
                return false;

            var library = client.GetLibrary(docId);
            var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/folders/{folderId}/documents/{docId}/move") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new { destination_folder_id = destinationFolderId }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> CopyDocument(this iManageClient client, string docId, string folderId)
        {
            if (string.IsNullOrEmpty(docId))
                return false;

            var library = client.GetLibrary(docId);
            var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/documents/{docId}/copy") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new { folder_id = folderId }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public static bool IsImage(this Document document) => ImageDocuments.Any(s => string.Equals(s, document.Type, StringComparison.OrdinalIgnoreCase));

        public static string GetIcon(this Document document) => DocumentIcons.Where(d =>
            string.Equals(d.Key, document.Type, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(d.Key, document.Extension, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;

        public static string GetFileName(this Document document) => string.Concat(document.Name, ".", document.Extension?.ToLower());

        public static async Task<HttpResponseMessage> GetDocument(this iManageClient client,
            string docId) //docId must include version number (ACTIVE_US!19453.1)
        {
            var library = client.GetLibrary(docId);
            var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/documents/{docId}/download"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new iManageServiceException(await response.GetErrorMessage(), response.StatusCode);

            return response;
        }

        public static async Task<MemoryStream?> GetDocumentAsStream(this iManageClient client,
            string docId) //docId must include version number (ACTIVE_US!19453.1)
        {
            var response = await client.GetDocument(docId);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return new MemoryStream(bytes);
        }

        public static async Task<FileContentResult> DownloadDocument(this iManageClient client, 
            string docId) //docId must include version number (ACTIVE_US!19453.1)
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

        public static async Task<string> GetContentAsStringAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            //todo: error handling
            //throw new iManageServiceException(await response.GetErrorMessage(), response.StatusCode);
            return "{}";
        }

        public static string GetLibrary(this iManageClient client, string? id = null)
        {
            var library = id?.Split('!')[0];

            if (string.IsNullOrEmpty(library))
                return client.Library;

            return library;
        }

        public static bool IsAuthCodeFlow(this iManageAuthenticationFlow authFlow) =>
            (authFlow == iManageAuthenticationFlow.AuthorizationCode ||
             authFlow == iManageAuthenticationFlow.Pkce);

        public static async Task<List<DocumentType>> GetTypes(this iManageClient client)
        {
            var library = client.GetLibrary();
            var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/types?limit=1000"); //no leading slash to use BaseAddress
            var response = await client.SendAsync(request);
            var result = await response.GetContentAsStringAsync();
            var typesResponse = JsonSerializer.Deserialize<DocumentTypesResponse>(result);

            if (typesResponse?.Data != null)
                return typesResponse.Data;

            return new List<DocumentType>();
        }

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

        public static async Task<List<Workspace>?> GetWorkspacesByName(this iManageClient client, string? name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var library = client.GetLibrary();
                var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/workspaces?name={name}"); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var workspacesResponse = JsonSerializer.Deserialize<WorkspacesResponse>(result);
                if (workspacesResponse?.Data != null)
                {
                    return workspacesResponse.Data.Results;
                }
            }

            return new List<Workspace>();
        }

        //public static async Task<Workspace> GetWorkspace(this iManageClient client, string? workspaceId)
        //{
        //    if (!string.IsNullOrEmpty(workspaceId))
        //    {
        //        var library = client.GetLibrary(workspaceId);
        //        var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/workspaces/{workspaceId}"); //no leading slash to use BaseAddress
        //        var response = await client.SendAsync(request);

        //        var result = await response.GetContentAsStringAsync();
        //        var workspaceResponse = JsonSerializer.Deserialize<WorkspaceResponse>(result);
        //        if (workspaceResponse?.Data != null)
        //        {
        //            return workspaceResponse.Data;
        //        }
        //    }

        //    return new Workspace();
        //}

        public static async Task<Folder> GetFolder(this iManageClient client, string? folderId)
        {
            if (!string.IsNullOrEmpty(folderId))
            {
                var library = client.GetLibrary(folderId);
                var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/folders/{folderId}"); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var folderResponse = JsonSerializer.Deserialize<FolderResponse>(result);
                if (folderResponse?.Data != null)
                {
                    return folderResponse.Data;
                }
            }

            return new Folder();
        }

        //public static async Task<Workspace?> GetWorkspaceByContainerId(this iManageClient client, string containerId)
        //{
        //    var workspace = new Workspace();
        //    var containerResult = await client.GetContainerResponseAsString(containerId);
        //    var containerResponse = JsonSerializer.Deserialize<ContainerResponse>(containerResult);
        //    if (containerResponse?.Data != null)
        //    {
        //        var container = containerResponse.Data;
        //        if (container.ContainerType == ContainerType.Workspace)
        //        {
        //            var workspaceResponse = JsonSerializer.Deserialize<WorkspaceResponse>(containerResult);
        //            workspace = workspaceResponse?.Data;
        //        }
        //        else
        //        {
        //            var folderResponse = JsonSerializer.Deserialize<FolderResponse>(containerResult);
        //            var folder = folderResponse?.Data;
        //            workspace = await client.GetWorkspace(folder?.WorkspaceId);
        //        }
        //    }

        //    return workspace;
        //}

        //public static async Task<string> GetLibraries(this iManageClient client)
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Get, "libraries"); //no leading slash to use BaseAddress
        //    var response = await client.SendAsync(request);

        //    return await response.GetContentAsStringAsync();
        //}

        //public static async Task<string> GetWorkspaces(this iManageClient client)
        //{
        //    var library = client.GetLibrary();
        //    var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/workspaces"); //no leading slash to use BaseAddress
        //    var response = await client.SendAsync(request);

        //    return await response.GetContentAsStringAsync();
        //}

        /// <summary>
        /// Get container profile
        /// </summary>
        /// <param name="client"></param>
        /// <param name="containerId"></param>
        /// <returns>JSON string response that can be serialized to Workspace or Folder object</returns>
        //public static async Task<string> GetContainerResponseAsString(this iManageClient client, string? containerId)
        //{
        //    var stringResponse = "{}";

        //    if (!string.IsNullOrEmpty(containerId))
        //    {
        //        var library = client.GetLibrary(containerId);
        //        var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/containers/{containerId}"); //no leading slash to use BaseAddress
        //        var response = await client.SendAsync(request);

        //        stringResponse = await response.GetContentAsStringAsync();
        //    }

        //    return stringResponse;
        //}

        /// <summary>
        /// Get search folder profile
        /// </summary>
        /// <param name="client"></param>
        /// <param name="searchFolderId"></param>
        /// <returns></returns>
        public static async Task<SearchFolder> GetSearchFolder(this iManageClient client, string? searchFolderId)
        {
            if (!string.IsNullOrEmpty(searchFolderId))
            {
                var library = client.GetLibrary(searchFolderId);
                var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/search-folders/{searchFolderId}"); //no leading slash to use BaseAddress
                var response = await client.SendAsync(request);

                var result = await response.GetContentAsStringAsync();
                var folderResponse = JsonSerializer.Deserialize<SearchFolderResponse>(result);
                if (folderResponse?.Data != null)
                {
                    return folderResponse.Data;
                }
            }

            return new SearchFolder();
        }

        /// <summary>
        /// Creates a new search folder.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="parentContainerId"></param>
        /// <param name="parentContainerType"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static async Task<SearchFolder?> CreateSearchFolder(this iManageClient client, string parentContainerId, ContainerType parentContainerType, SearchFolder template)
        {
            var library = client.GetLibrary(parentContainerId);
            var requestUri = parentContainerType == ContainerType.Workspace ?
                $"libraries/{library}/workspaces/{parentContainerId}/search-folders" :
                $"libraries/{library}/folders/{parentContainerId}/search-folders";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    name = template.Name,
                    description = template.Description,
                    searchprofile = template.SearchProfile,
                    default_security = template.DefaultSecurity
                }, options: new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            var result = await response.GetContentAsStringAsync();
            var folderResponse = JsonSerializer.Deserialize<SearchFolderResponse>(result);
            if (folderResponse?.Data != null)
                return folderResponse.Data;

            return null;
        }

        /// <summary>
        /// Creates a new workspace tab.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="workspaceId"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static async Task<Folder?> CreateWorkspaceTab(this iManageClient client, string workspaceId, Folder template)
        {
            var library = client.GetLibrary(workspaceId);
            var request = new HttpRequestMessage(HttpMethod.Post, $"libraries/{library}/workspaces/{workspaceId}/tabs") //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new 
                { 
                    name = template.Name,
                    description = template.Description,
                    default_security = template.DefaultSecurity
                }, options: new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            var result = await response.GetContentAsStringAsync();
            var folderResponse = JsonSerializer.Deserialize<FolderResponse>(result);
            if (folderResponse?.Data != null)
                return folderResponse.Data;

            return null;
        }

        /// <summary>
        /// Creates a new folder within another folder.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="parentContainerId"></param>
        /// <param name="parentContainerType"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static async Task<Folder?> CreateFolder(this iManageClient client, string parentContainerId, ContainerType parentContainerType, Folder template)
        {
            var library = client.GetLibrary(parentContainerId);
            var requestUri = parentContainerType == ContainerType.Workspace ?
                $"libraries/{library}/workspaces/{parentContainerId}/folders" :
                $"libraries/{library}/folders/{parentContainerId}/folders";            
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) //no leading slash to use BaseAddress
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    name = template.Name,
                    description = template.Description,
                    profile = template.Profile,
                    default_security = template.DefaultSecurity
                }, options: new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            var result = await response.GetContentAsStringAsync();
            var folderResponse = JsonSerializer.Deserialize<FolderResponse>(result);
            if (folderResponse?.Data != null)
            {
                var newFolder = folderResponse.Data;

                //update email
                if (newFolder != null && !string.IsNullOrEmpty(template.Email) && !string.IsNullOrEmpty(newFolder.Name) && !string.IsNullOrEmpty(newFolder.Id))
                {
                    var email = GetEmail(newFolder.Name, newFolder.Id);

                    request = new HttpRequestMessage(HttpMethod.Patch, $"libraries/{library}/folders/{newFolder.Id}") //no leading slash to use BaseAddress
                    {
                        Content = new StringContent(JsonSerializer.Serialize(new { email = email}), Encoding.UTF8, "application/json")
                    };
                    response = await client.SendAsync(request);
                    result = await response.GetContentAsStringAsync();
                    folderResponse = JsonSerializer.Deserialize<FolderResponse>(result);
                    if (folderResponse?.Data != null)
                        newFolder = folderResponse.Data;
                }

                return newFolder;
            }

            return null;
        }

        /// <summary>
        /// Get email using format: {folder_name}.{id}
        /// Updating the folder automatically adds .{library}@{domain} (.Dev@mail.cloudimanage.com) 
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public static string GetEmail(string folderName, string folderId)
        {
            var name = folderName.ToLower().Replace(" ", "_");
            StringBuilder sb = new StringBuilder();
            foreach (char c in name)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
            }
            name = sb.ToString();

            return $"{name}.{folderId.Split('!')[1]}";
        }
    }
}
