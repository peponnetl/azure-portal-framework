using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.StorageAccount
{
    public interface IFileStorageRepository
    {
        Task<bool> DoesFileExistsAsync(string accountName, string containerName, string fileName);

        Task<IEnumerable<IAzureFileInfo>> GetFileListAsync(string accountName, string containerName, string filePrefix, Dictionary<string, string> metadataToPropertyMapping);

        Task<IAzureFile> DownloadFileAsync(string accountName, string containerName, string fileFullName);

        Task AddFileAsync(string accountName, string containerName, string fileName, byte[] fileBytes);

        Task SnapshotFileAsync(string accountName, string containerName, string fileName);
    }
}
