using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.StorageAccount
{
    public interface IAzureFile
    {
        string FileName { get; }

        Stream Stream { get; }

        string ContentType { get; }
    }
}
