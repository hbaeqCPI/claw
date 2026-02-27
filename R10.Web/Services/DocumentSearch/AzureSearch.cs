using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Kendo.Mvc.Extensions;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest.Azure;
using R10.Core.DTOs;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;

using R10.Web.Services.DocumentStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace R10.Web.Services.DocumentSearch
{
    public class AzureSearch
    {
        protected readonly ISystemSettings<DefaultSetting> _settings;
        protected readonly IConfiguration _configuration;
        private readonly AzureStorage _azureStorage;
        private const int MAX_AZ_RECORD_COUNT = 1000;

        public AzureSearch(
                ISystemSettings<DefaultSetting> settings,
                IConfiguration configuration,
                AzureStorage azureStorage)
        {
            _settings = settings;
            _configuration = configuration;
            _azureStorage = azureStorage;
        }

        #region Search
        public List<GSDocParamDTO> SearchDocument(List<AzureSearchDocList> docList)
        {
            var gsSettings = _configuration.GetSection("GlobalSearch").Get<GlobalSearchSettings>();

            var searchApiKey = gsSettings.ApiKey;
            var searchIndexName = GetSearchIndexName(gsSettings);
            var searchUrl = string.Format(gsSettings.Url, gsSettings.ServiceName);
            Uri serviceEndpoint = new Uri(searchUrl);

            SearchClient searchClient = new SearchClient(
               serviceEndpoint,
               searchIndexName,
               new AzureKeyCredential(searchApiKey)
           );

            int recordId = 0;                               // use for processing with SQL data later
            var docResults = new List<GSDocParamDTO>();

            docList.Each(docSearch => {
                SearchOptions options = new SearchOptions()
                {
                    Size = MAX_AZ_RECORD_COUNT,                     // max = 1000
                    //Skip = data.Skip,                             // default = 0
                    IncludeTotalCount = true,
                    Filter = CreateSearchFilter(docSearch.SystemScreens, docSearch.DocumentTypes),
                    SearchMode = docSearch.DocSearchMode == "any" ? SearchMode.Any : SearchMode.All,
                    QueryType = docSearch.DocQueryType == "full" ? SearchQueryType.Full : SearchQueryType.Simple

                };

                SearchResults<SearchDocument> response = searchClient.Search<SearchDocument>(docSearch.Criteria, options);

                //var facetOutput = new Dictionary<String, IList<FacetValue>>();
                //foreach (var facetResult in response.Facets)
                //{
                //    facetOutput[facetResult.Key] = facetResult.Value
                //               .Select(x => new FacetValue() { value = x.Value.ToString(), count = x.Count })
                //               .ToList();
                //}

                var output = new SearchOutput
                {
                    Count = response.TotalCount,
                    Results = response.GetResults().ToList(),
                    //Facets = facetOutput
                };


                output.Results.ForEach(item => {
                    var result = new GSDocParamDTO()
                    {
                        RecordId = ++recordId,
                        SystemType = item.Document["SystemType"].ToString(),
                        ScreenCode = item.Document["ScreenCode"].ToString(),
                        ParentId = Convert.ToInt32(item.Document["ParentId"]),
                        DocumentType = item.Document["DocumentType"].ToString(),
                        FilePath = item.Document["metadata_storage_path"].ToString(),
                        FileName = item.Document["metadata_storage_name"].ToString(),
                        SearchScore = Convert.ToDecimal(item.Score)
                    };
                    if (item.Document.ContainsKey("LogId"))
                        result.LogId = Convert.ToInt32(item.Document["LogId"]);

                    docResults.Add(result);
                });
            });

            return docResults;
        }

        private string CreateSearchFilter(string systemScreens, string documentTypes)
        {
            List<string> filterExpr = new List<string>();

            systemScreens = systemScreens.Substring(1, systemScreens.Length - 2);
            documentTypes = documentTypes.Substring(1, documentTypes.Length - 2);
            var screenArray = systemScreens.Split("|");
            var docTypeArray = documentTypes.Split("|");

            screenArray.Each(scr => {
                var sysScreen = scr.Split("-");
                docTypeArray.Each(docType => {
                    filterExpr.Add($"(SystemType eq '{sysScreen[0]}' and ScreenCode eq '{sysScreen[1]}' and DocumentType eq '{docType}')");

                });
            });

            return string.Join (" or ", filterExpr);
        }
        #endregion

        #region Provision Search Index
        //public async Task<string> CreateAzureSearchIndex(bool updateIfExists)
        //{
        //    var gsSettings = _configuration.GetSection("GlobalSearch").Get<GlobalSearchSettings>();

        //    var searchServiceName = gsSettings.ServiceName;
        //    var adminApiKey = gsSettings.AdminKey;
        //    SearchServiceClient searchService = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

        //    var indexMsg = await CreateSearchIndex(searchService, updateIfExists);
        //    var indexerMsg = await CreateSearchIndexer(searchService, gsSettings, updateIfExists);
        //    return indexMsg + "; " + indexerMsg;
        //}

        //private async Task<string> CreateSearchIndex(SearchServiceClient searchService, bool updateIfExists)
        //{
        //    var gsSettings = _configuration.GetSection("GlobalSearch").Get<GlobalSearchSettings>();
        //    var searchIndexName = GetSearchIndexName(gsSettings);
        //    bool indexExists = searchService.Indexes.Exists(searchIndexName);
        //    if (indexExists)
        //    {
        //        if (updateIfExists)
        //        {
        //            await searchService.Indexes.DeleteAsync(searchIndexName);       // delete existing index
        //        }
        //        else
        //        {
        //            //throw new Exception($"Azure Search Index already exists. Please check 'Re-create Index If Exists' if you want to delete existing and re-create it: {searchIndexName}");
        //            return $"Index already existing, creation skipped: {searchIndexName}";
        //        }
        //    }

        //    var indexDefinition = new Microsoft.Azure.Search.Models.Index()
        //    {
        //        Name = searchIndexName,
        //        Fields = FieldBuilder.BuildForType<GlobalSearchIndex>()
        //    };
        //    searchService.Indexes.Create(indexDefinition);

        //    return $"Index created: {searchIndexName}";
        //}

        //private async Task<string> CreateSearchIndexer(SearchServiceClient searchService, GlobalSearchSettings settings, bool updateIfExists)
        //{
        //    var searchIndexerName = GetSearchIndexerName(settings);

        //    bool indexerExists = await searchService.Indexers.ExistsAsync(searchIndexerName);
        //    if (indexerExists)
        //    {
        //        if (updateIfExists)
        //            await searchService.Indexers.ResetAsync(searchIndexerName);
        //        else
        //        {
        //            //throw new Exception($"Azure Search Indexer already exists. Please check 'Re-create Index If Exists' if you want to reset and update: {blobStorageIndexer.Name}");
        //            return $"Indexer already existing, creation skipped: {searchIndexerName}";
        //        }
        //    }

        //    // create datasource, this has the connection string to the blob storage
        //    var blobStorageKey = _configuration.GetSection("BlobStorage:AccessKey").Get<string>();
        //    var storageSettings = _configuration.GetSection("DocumentStorage").Get<DocumentStorageSettings>();
        //    var connString = string.Format(storageSettings.StorageConnectionString, storageSettings.StorageAccountName, blobStorageKey);
        //    var blobContainerName = _azureStorage.GetBlobContainerName(storageSettings);
        //    var dataSourceName = blobContainerName + "-ds";
        //    DataSource blobStorageDataSource = DataSource.AzureBlobStorage(dataSourceName, connString, blobContainerName);

        //    await searchService.DataSources.CreateOrUpdateAsync(blobStorageDataSource);

        //    // define indexer configuration; this matches demo index settings
        //    IDictionary<string, object> config = new Dictionary<string, object>();
        //    config.Add(key: "dataToExtract", value: "contentAndMetadata");
        //    config.Add(key: "imageAction", value: "generateNormalizedImages");
        //    config.Add(key: "parsingMode", value: "default");

        //    // this matches input mappings of demo indexer
        //    List<FieldMapping> fieldMappings = new List<FieldMapping>();
        //    fieldMappings.Add(new FieldMapping(
        //        sourceFieldName: "metadata_storage_path",
        //        targetFieldName: "metadata_storage_path",
        //        mappingFunction: new FieldMappingFunction(
        //            name: "base64Encode")));
        //    //fieldMappings.Add(new FieldMapping(
        //    //    sourceFieldName: "content",
        //    //    targetFieldName: "content"));

        //    // this matches output mappings of demo indexer
        //    List<FieldMapping> outputMappings = new List<FieldMapping>();
        //    outputMappings.Add(new FieldMapping(
        //        sourceFieldName: "/document/merged_content",
        //        targetFieldName: "merged_content"));
        //    outputMappings.Add(new FieldMapping(
        //        sourceFieldName: "/document/normalized_images/*/text",
        //        targetFieldName: "text"));
        //    outputMappings.Add(new FieldMapping(
        //        sourceFieldName: "/document/normalized_images/*/layoutText",
        //        targetFieldName: "layoutText"));

        //    var indexingSchedule = new TimeSpan(settings.IndexerExecDays, settings.IndexerExecHours, settings.IndexerExecMinutes, 0);
        //    Indexer blobStorageIndexer = new Indexer(
        //        name: searchIndexerName,
        //        dataSourceName: blobStorageDataSource.Name,
        //        targetIndexName: GetSearchIndexName(settings),
        //        skillsetName: settings.SkillsetName,
        //        parameters: new IndexingParameters(
        //            maxFailedItems: 0,
        //            maxFailedItemsPerBatch: 0,
        //            configuration: config),
        //        fieldMappings: fieldMappings,
        //        outputFieldMappings: outputMappings,
        //        schedule: new IndexingSchedule(indexingSchedule)
        //        );

           
        //    await searchService.Indexers.CreateOrUpdateAsync(blobStorageIndexer);

        //    try
        //    {
        //        await searchService.Indexers.RunAsync(blobStorageIndexer.Name);
        //    }
        //    catch (CloudException e) when (e.Response.StatusCode == (HttpStatusCode)429)
        //    {
        //        throw new Exception($"Failed to run Azure Search Indexer: {blobStorageIndexer.Name}\n\n{e.Response.Content}");
        //    }
        //    return $"Indexer created: {searchIndexerName}";
        //}

        #endregion


        #region Common
        private string GetSearchIndexName (GlobalSearchSettings settings)
        {
            var searchIndexName = settings.IndexName.ToLower();
            if (string.IsNullOrEmpty(searchIndexName)) { 
                searchIndexName = $"{_azureStorage.GetBlobContainerName()}-index";
            }
            return searchIndexName;
        }

        //private string GetSearchIndexerName(GlobalSearchSettings settings)
        //{
        //    var searchIndexerName = settings.IndexerName;
        //    if(string.IsNullOrEmpty(searchIndexerName))
        //    {
        //        searchIndexerName = $"{_azureStorage.GetBlobContainerName()}-indexer";
        //    }
        //    return searchIndexerName;
        //}
        #endregion
    }
}
