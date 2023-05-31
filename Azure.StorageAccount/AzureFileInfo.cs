using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.StorageAccount
{
    public class AzureFileInfo : IAzureFileInfo
    {
        public string Id { get; set; }

        public string FileName { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public DateTime Version { get; set; }

        public string Directory { get; set; }
    }
}
