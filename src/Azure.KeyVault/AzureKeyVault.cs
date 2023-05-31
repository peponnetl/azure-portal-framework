using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Azure.KeyVault
{
    public class AzureKeyVault : IAzureKeyVault
    {
        private SecretClient _secretClient = null!;

        public void ConfigureClient(string tenantId, string clientId, string clientSecret, string keyVaultUrl)
        {
            _secretClient = new SecretClient(
                new Uri(keyVaultUrl),
                new ClientSecretCredential(tenantId, clientId, clientSecret),
                new SecretClientOptions()
                {
                    Retry =
                    {
                        Delay= TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential
                    }
                }
            );
        }

        public async Task<string> GetSecretValueAsync(string secretName)
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            
            return secret.Value.Value;
        }

        public async Task<IDictionary<string, string>> GetSecretsValues()
        {
            var output = new Dictionary<string, string>();

            var secrets = _secretClient.GetPropertiesOfSecretsAsync();
            await foreach (var secretProperties in secrets)
            {
                var secretResponse = await _secretClient.GetSecretAsync(secretProperties.Name);

                output.Add(secretProperties.Name, secretResponse.Value.Value);
            }

            return output;
        }
    }
}