using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.StorageAccount
{
    public class IAzureFileInfo
    {
        string Id { get; }

        string FileName { get; }

        string DisplayName { get; }

        string Description { get; }

        DateTime Version { get; }

        string Directory { get; }
    }
}
