using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Azure.StorageAccount
{
    public class AzureStorage : IFileStorageRepository
    {
        private const string Delimiter = @"/";
        private readonly string _azureConnectionString;

        public AzureStorage(string connectionString)
        {
            _azureConnectionString = connectionString;
        }

        private static string GetFileName(string fileFullName)
        {
            var output = fileFullName;

            if (fileFullName.Contains(Delimiter))
            {
                var split = fileFullName.Split(Delimiter).ToList();
                output = split.Last();
            }

            return output;
        }

        private string GetFileDirectory(string fileName)
        {
            var folders = fileName.Split(Delimiter).ToList();

            return !folders.Any() 
                ? string.Empty 
                : string.Join(Delimiter, folders.Take(folders.Count - 1).ToArray());
        }

        public async Task<IEnumerable<IAzureFileInfo>> GetFileListAsync(string accountName, string containerName, string filePrefix, Dictionary<string, string> metadataToPropertyMapping)
        {
            var files = new List<IAzureFileInfo>();

            var blobServiceClient = new BlobServiceClient(_azureConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await foreach (var blobItem in blobContainerClient.GetBlobsAsync(prefix: filePrefix))
            {
                var blob = blobContainerClient.GetBlobClient(blobItem.Name);

                var file = new AzureFileInfo
                {
                    Id = blobItem.Name,
                    FileName = blobItem.Name.Replace(filePrefix, string.Empty),
                    Directory = containerName,
                    Version = blobItem.Properties.LastModified?.LocalDateTime ?? blobItem.Properties.CreatedOn.Value.LocalDateTime
                };

                if (metadataToPropertyMapping.Keys.Count == 0)
                {
                    files.Add(file);
                    continue;
                }

                var blobProperties = await blob.GetPropertiesAsync();
                foreach (var mapping in metadataToPropertyMapping.Keys)
                {
                    if (blobProperties.Value.Metadata.ContainsKey(mapping))
                    {
                        var propertyInfo = typeof(AzureFileInfo).GetProperty(metadataToPropertyMapping[mapping]);
                        propertyInfo.SetValue(file, blobProperties.Value.Metadata[mapping]);
                    }
                }

                files.Add(file);
            }

            return files;
        }

        public async Task<IAzureFile> DownloadFileAsync(string accountName, string containerName, string fileFullName)
        {
            var blobServiceClient = new BlobServiceClient(_azureConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileFullName);

            BlobDownloadResult blobDownloadResult;
            try
            {
                blobDownloadResult = await blobClient.DownloadContentAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not download '{fileFullName}' from '{containerName}'", ex);
            }

            var file = new AzureFile()
            {
                FileName = GetFileName(blobClient.Name),
                ContentType = blobDownloadResult.Details.ContentType,
                Stream = blobDownloadResult.Content.ToStream()
            };

            file.Stream.Position = 0;

            return file;
        }

        public async Task AddFileAsync(string accountName, string containerName, string fileName, byte[] fileBytes)
        {
            var blobServiceClient = new BlobServiceClient(_azureConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var binaryData = new BinaryData(fileBytes);

            await blobClient.UploadAsync(binaryData, true);
        }

        public async Task SnapshotFileAsync(string accountName, string containerName, string fileName)
        {
            var blobServiceClient = new BlobServiceClient(_azureConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.CreateSnapshotAsync();
        }

        public async Task<bool> DoesFileExistsAsync(string accountName, string containerName, string fileName)
        {
            var fileRepository = GetFileDirectory(fileName);

            var results = await GetFileListAsync(accountName, containerName, fileRepository, new Dictionary<string, string>());

            return results.ToList().Any(f => ((AzureFileInfo)f).Id == fileName);
        }
    }
}