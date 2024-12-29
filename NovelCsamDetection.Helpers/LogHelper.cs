namespace NovelCsamDetection.Helpers
{
    public class LogHelper(ILoggerFactory loggerFactory, TelemetryClient telemetryClient) : ILogHelper
    {
        private readonly ILogger<LogHelper> _logger = loggerFactory.CreateLogger<LogHelper>();
        private readonly TelemetryClient _telemetryClient = telemetryClient;

        #region Information Logging

        public void LogInformation(string message, string sourceClassName, string sourceFunction)
        {
            var logMessage = $"{sourceClassName}:{sourceFunction}:{message}";
            _logger.LogInformation(logMessage);
        }

        #endregion

        #region Exception Logging

        public void LogException(string message, string sourceClassName, string sourceFunction, Exception ex)
        {
            var logMessage = $"{sourceClassName}:{sourceFunction}:{message}:{ex.Message}";
            _logger.LogError(logMessage);
            _telemetryClient.TrackException(ex, new Dictionary<string, string> {
                { "Custom Message", message },
                { "sourceClassName", sourceClassName },
                { "sourceFunction", sourceFunction }});
        }


        #endregion

        #region Trace Logging
        public void LogTrace(string message, string sourceClassName, string sourceFunction)
        {
            var logMessage = $"{sourceClassName}:{sourceFunction}:{message}";
            _logger.LogTrace(logMessage);
            _telemetryClient.TrackTrace(logMessage, new Dictionary<string, string> {
                { "Custom Message", message },
                { "sourceClassName", sourceClassName },
                { "sourceFunction", sourceFunction }});
        }
        #endregion
    }
}
