namespace Azure.LogAnalyticsWorkspace
{
    public interface IAzureLogger
    {
        void LogInfo(string logName, string username, string message, string jsonPayload);

        void LogError(string logName, string username, string message, Exception exception, string jsonPayload);
    }
}
