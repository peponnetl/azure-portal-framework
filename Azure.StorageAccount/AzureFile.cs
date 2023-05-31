namespace Azure.StorageAccount
{
    public class AzureFile : IAzureFile
    {
        public string FileName { get; set; }

        public Stream Stream { get; set; }

        public string ContentType { get; set; }
    }
}
