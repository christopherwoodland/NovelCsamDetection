namespace NovelCsamDetection.Helper
{
    public class AzureContainerAppJobHelper : IAzureContainerAppJobHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogHelper _logHelper;
        private readonly IAppConfigurationHelper _appConfigurationHelper;
        public AzureContainerAppJobHelper(IAppConfigurationHelper appConfigurationHelper, ILogHelper logHelper, HttpClient httpClient)
        {
            _appConfigurationHelper = appConfigurationHelper;
            _logHelper = logHelper;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(18000);
        }
        public async Task<bool> StartExtractJobAsync(string subscriptionId, string resourceGroupName, string containerAppName, string jobName, string accessToken)
        {
            //var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Web/containerApps/{containerAppName}/jobs/{jobName}/start?api-version=2022-03-01";
            var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.App/jobs/{jobName}/start?api-version=2023-05-01";
            var requestBody = new
            {
                properties = new
                {
                    env = new[]
                    {
                    new { name = "FILE_NAME_PARAM", value = "short.webm" },
                    new { name = "KEY_VAULT_PARAM", value = "digitalforensicskv" },
                    new { name = "SOURCE_CONTAINER_NAME_PARAM", value = "videos" },
                    new { name = "SOURCE_DIR_PATH_PARAM", value = "input" },
                    new { name = "DEST_DIR_PATH_EXT_PARAM", value = "extracted" },
                    new { name = "STORAGE_ACCOUNT_NAME_PARAM", value = "digitalforensicsstg" },
                    new { name = "DEBUG_MODE_PARAM", value = "True" }
                }
                }
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


            // Define retry policy
            //RetryPolicy<HttpResponseMessage> retryPolicy = Policy
            // .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            // .Or<HttpRequestException>()
            // .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));



            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var message = $"Job started: {response.StatusCode} {responseContent}";
                _logHelper.LogInformation(message, "AzureContainerAppJobHelper", "StartExtractJobAsync");
                return true;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var message = $"Failed to start job. Status code: {response.StatusCode} {responseContent}";
                _logHelper.LogException(message, "AzureContainerAppJobHelper", "StartExtractJobAsync", new Exception(message));
                return false;

            }
        }
    }
}
