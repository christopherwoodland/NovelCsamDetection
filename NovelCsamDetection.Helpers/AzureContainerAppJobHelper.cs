
using NovelCsam.Models;
using System.Diagnostics;

namespace NovelCsamDetection.Helpers
{
    public class AzureContainerAppJobHelper : IAzureContainerAppJobHelper
    {
        private readonly static HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(300)
        };
        private readonly ILogHelper _logHelper;
        public AzureContainerAppJobHelper(ILogHelper logHelper)
        {
            _logHelper = logHelper;
        }
        public async Task<bool> StartExtractJobAsync(string accessToken, ExtractParameters ep)
        {
            var url = $"https://management.azure.com/subscriptions/{ep.SubscriptionParam}/resourceGroups/{ep.ResourceGroupParam}/providers/Microsoft.App/jobs/{ep.JobNameParam}/start?api-version=2023-05-01";
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ep);
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




        public async Task<bool> StartAnalyzeJobAsync(string accessToken, AnalyzeParameters aprams)
        {
            var url = $"https://management.azure.com/subscriptions/{aprams.SubscriptionParam}/resourceGroups/{aprams.ResourceGroupParam}/providers/Microsoft.App/jobs/{aprams.JobNameParam}/start?api-version=2023-05-01";

            var requestBody = new
            {
                properties = new
                {
                    env = new[]
                  {
                        new { name = "FILE_NAME_PARAM", value = aprams.FileNameParam},
                        new { name = "KEY_VAULT_NAME_PARAM", value = aprams.KeyVaultParam},
                        new { name = "STORAGE_ACCOUNT_NAME_PARAM", value = aprams.StorageAccountNameParam},
                        new { name = "SOURCE_CONTAINER_NAME_PARAM", value = aprams.SourceContainerNameParam },
                        new { name = "SOURCE_DIR_PATH_PARAM", value = aprams.SourceContainerNameParam },
                        new { name = "DEST_DIR_PATH_PARAM", value = aprams.DestDirPathParam },
                        new { name = "DEST_DIR_PATH_EXT_PARAM", value = aprams.DestDirPathExtParam },
                        new { name = "COSMOS_DB_NAME_PARAM", value = aprams.CosmosDbNameParam },
                        new { name = "COSMOS_DB_CONTAINER_NAME_PARAM", value = aprams.CosmosDbContainerNameParam },
                        new { name = "DEBUG_MODE_PARAM", value = aprams.DebugModeParam },
                        new { name = "WRITE_TO_COSMOS", value = aprams.WriteToCosmosParam },
                        new { name = "WRITE_RESULT_TO_JSON_PARAM", value = aprams.WriteToCosmosParam}
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
                _logHelper.LogInformation(message, "AzureContainerAppJobHelper", "StartAnalyzeJobAsync");
                return true;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var message = $"Failed to start job. Status code: {response.StatusCode} {responseContent}";
                _logHelper.LogException(message, "AzureContainerAppJobHelper", "StartAnalyzeJobAsync", new Exception(message));
                return false;

            }
        }



        //public async Task<bool> StartAnalyzeJobAsync(string subscriptionId, string resourceGroupName, string jobName, string accessToken,
        //  string fileName, string keyVault, string sourseContainerName,
        //  string sourceDir, string extDir, string resultDir,
        //  string storageAccountName, string cosmosDbName, string cosmosDbContainerName,
        //  bool debug = true, bool writeToJson = false,
        //  bool writeToCosmos = false)
        //{
        //    var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.App/jobs/{jobName}/start?api-version=2023-05-01";
        //    var requestBody = new
        //    {
        //        properties = new
        //        {
        //            env = new[]
        //            {

        //            new { name = "FILE_NAME_PARAM", value = fileName},
        //            new { name = "KEY_VAULT_PARAM", value = keyVault },
        //            new { name = "SOURCE_CONTAINER_NAME_PARAM", value = sourseContainerName},
        //            new { name = "SOURCE_DIR_PATH_PARAM", value = sourceDir },
        //            new { name = "DEST_DIR_PATH_EXT_PARAM", value = extDir },
        //            new { name = "DEST_DIR_PATH_PARAM", value = resultDir },
        //            new { name = "STORAGE_ACCOUNT_NAME_PARAM", value = storageAccountName },
        //            new { name = "DEBUG_MODE_PARAM", value = debug },
        //            new { name = "WRITE_TO_COSMOS_PARAM", value = writeToCosmos },
        //            new { name = "WRITE_RESULT_TO_JSON_PARAM", value = writeToJson },
        //            new { name = "COSMOS_DB_NAME_PARAM", value = cosmosDbName },
        //            new { name = "COSMOS_DB_CONTAINER_NAME_PARAM", value = cosmosDbContainerName }
        //        }
        //        }
        //    };

        //    var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


        //    // Define retry policy
        //    //RetryPolicy<HttpResponseMessage> retryPolicy = Policy
        //    // .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        //    // .Or<HttpRequestException>()
        //    // .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));



        //    var response = await _httpClient.PostAsync(url, content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        var message = $"Job started: {response.StatusCode} {responseContent}";
        //        _logHelper.LogInformation(message, "AzureContainerAppJobHelper", "StartAnalyzeJobAsync");
        //        return true;
        //    }
        //    else
        //    {
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        var message = $"Failed to start job. Status code: {response.StatusCode} {responseContent}";
        //        _logHelper.LogException(message, "AzureContainerAppJobHelper", "StartAnalyzeJobAsync", new Exception(message));
        //        return false;

        //    }
        //}
    }
}
