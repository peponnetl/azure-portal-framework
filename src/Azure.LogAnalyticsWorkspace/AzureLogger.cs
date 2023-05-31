using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using log4net.Core;

namespace Azure.LogAnalyticsWorkspace
{
    public class AzureLogger : IAzureLogger
    {
        private readonly string _workspaceId;
        private readonly string _sharedKey;

        private readonly BlockingCollection<LogEvent> _logEvent = new();

        protected virtual void Enqueue(LogEvent logEvent)
        {
            _logEvent.Add(logEvent);
        }

        private void OnHandlerStart()
        {
            foreach (var job in _logEvent.GetConsumingEnumerable(CancellationToken.None))
            {
                Task.Run(async () => await Post(job, _workspaceId, _sharedKey))
                    .ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private static async Task Post(LogEvent logEvent, string workspaceId, string sharedKey)
        {
            string json;
            try
            {
                json = JsonSerializer.Serialize(logEvent);
            }
            catch (Exception e)
            {
                json = $"Could not serialize LogEvent ({e.Message})";
            }

            var datestring = DateTime.UtcNow.ToString("r");
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            var stringToHash = $"POST\n{jsonBytes.Length}\napplication/json\nx-ms-date:{datestring}\n/api/logs";

            var keyByte = Convert.FromBase64String(sharedKey);
            var messageBytes = new ASCIIEncoding().GetBytes(stringToHash);

            var hashedString = string.Empty;
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                var hash = hmacsha256.ComputeHash(messageBytes);
                hashedString = Convert.ToBase64String(hash);
            }

            var signature = $"SharedKey {workspaceId}:{hashedString}";

            var url = $"https://{workspaceId}.ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Log-Type", logEvent.LoggerName);
            client.DefaultRequestHeaders.Add("Authorization", signature);
            client.DefaultRequestHeaders.Add("x-ms-date", datestring);

            var httpContent = new StringContent(json, Encoding.UTF8);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            await client.PostAsync(new Uri(url), httpContent);
        }

        public AzureLogger(string workspaceId, string sharedKey, int numThreads = 4)
        {
            if (string.IsNullOrEmpty(workspaceId))
                throw new ArgumentException("Can not be empty", nameof(workspaceId));

            _workspaceId = workspaceId;

            if (string.IsNullOrEmpty(sharedKey))
                throw new ArgumentException("Can not be empty", nameof(sharedKey));

            _sharedKey = sharedKey;

            if (numThreads <= 0)
                throw new ArgumentException("Must be greater than 0", nameof(numThreads));

            for (var i = 0; i < numThreads; i++)
            {
                var thread = new Thread(OnHandlerStart) { IsBackground = true };
                thread.Start();
            }
        }

        public void LogInfo(string logName, string username, string message, string jsonPayLoad) => Enqueue(new LogEvent(logName, Level.Debug, username, message, null!, jsonPayLoad));

        public void LogError(string logName, string username, string message, Exception exception, string jsonPayLoad) => Enqueue(new LogEvent(logName, Level.Error, username, message, exception, jsonPayLoad));
    }
}
