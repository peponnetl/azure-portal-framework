using log4net.Core;
using System.Runtime.Serialization;

namespace Azure.LogAnalyticsWorkspace
{
    [Serializable]
    public class LogEvent
    {
        public LogEvent(string loggerName, Level level, string userName, string message, Exception exceptionObject, string rawData)
        {
            LoggerName = loggerName;
            LevelName = level.Name;
            ExceptionObject = exceptionObject;
            Message = message;
            UserName = userName;
            RawData = rawData;
            _Timestamp = DateTime.Now;
        }

        [IgnoreDataMember]
        private DateTime _Timestamp { get; set; }

        [DataMember]
        public string Timestamp
        {
            get
            {
                return _Timestamp.ToString();
            }
        }

        [DataMember]
        public string LoggerName { get; set; }

        [DataMember]
        public string LevelName { get; }

        [DataMember]
        public string UserName { get; }

        [DataMember]
        public string Message { get; }

        [IgnoreDataMember]
        private Exception? ExceptionObject { get; }

        [DataMember]
        public string Exception
        {
            get
            {
                var message = $"Exception {LogExceptionDetails(ExceptionObject)}";
                if (ExceptionObject?.InnerException != null)
                {
                    var baseException = ExceptionObject.GetBaseException();
                    message += $"\nBase Exception {LogExceptionDetails(baseException)}";
                }
                return message;
            }
        }

        private string LogExceptionDetails(Exception? ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }
            return $"Message={ex.Message}\nStacktrace={ex.StackTrace}";
        }

        [DataMember]
        public string RawData { get; }
    }
}
