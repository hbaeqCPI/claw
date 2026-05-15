namespace LawPortal.Web.Services.DocumentStorage
{
    // Bound from appsettings.json "DocumentStorage" section.
    // UseFileSystem=true → FileSystemStorage; otherwise AzureStorage (Azure Blob).
    public class DocumentStorageSettings
    {
        public bool UseFileSystem { get; set; }
        public bool UseAzureStorage { get; set; }
        public string StorageADTenantID { get; set; } = "";
        public string StorageAppClientID { get; set; } = "";
        public string StorageAppClientSecret { get; set; } = "";
        public string StorageAccountName { get; set; } = "";
        public string StorageContainerName { get; set; } = "";
        public string StorageUrl { get; set; } = "https://{0}.blob.core.windows.net/{1}";
        public string StorageConnectionString { get; set; } = "";
    }
}
