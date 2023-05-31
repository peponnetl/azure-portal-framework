using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.KeyVault
{
    public interface IAzureKeyVault
    {
        Task<string> GetSecretValueAsync(string secretName);

        Task<IDictionary<string, string>> GetSecretsValues();
    }
}
