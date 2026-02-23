using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class DocumentStorageSettings
    {
        public bool UseFileSystem { get; set; }
        public bool UseAzureStorage { get; set; }

        public string StorageADTenantID { get; set; }
        public string StorageAppClientID { get; set; }
        public string StorageAppClientSecret { get; set; }

        public string StorageAccountName { get; set; }
        public string StorageContainerName { get; set; }
        public string StorageUrl { get; set; }
        public string StorageConnectionString { get; set; }
        
    }
}
