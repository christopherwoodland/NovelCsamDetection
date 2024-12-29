using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace NovelCsamDetection.Functions
{
    public class NovelCsam
    {
        private readonly ILogger<NovelCsam> _logger;
        private readonly IAzureContainerAppJobHelper _jobHelper;
        public NovelCsam(ILogger<NovelCsam> logger, IAzureContainerAppJobHelper jobHelper)
        {
            _logger = logger;
            _jobHelper = jobHelper;
        }

        [Function("RunNovelCsamStartExtractJobAsync")]
        public async Task<HttpResponseData> RunNovelCsamStartExtractJobAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "jobs/extract")] HttpRequestData req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<ExtractParameters>(requestBody);
                if (data == null)
                {
                    var message = $"Could not process the video, data is null";
                    _logger.LogError(message, "RunNovelCsamStartExtractJobAsync", new Exception(message));
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }
                var fileName = data.FileNameParam;
                if (string.IsNullOrEmpty(fileName))
                {
                    var message = $"Could not process the video {fileName} was null or empty";
                    _logger.LogError(message, "RunNovelCsamStartExtractJobAsync", new Exception(message));
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                _logger.LogInformation($"RunVideoHelperCreateFramesJobAsync Name: {fileName}");

                var accessToken = await AccountTokenProvider.GetAccountAccessTokenAsync(_logger).ConfigureAwait(false);
                var ep = new ExtractParameters
                {
                    DebugModeParam = data.DebugModeParam,
                    DestDirPathExtParam = data.DestDirPathExtParam,
                    SourceContainerNameParam = data.SourceContainerNameParam,
                    FileNameParam = fileName,
                    JobNameParam = data.JobNameParam,
                    KeyVaultParam = data.KeyVaultParam,
                    ResourceGroupParam = data.ResourceGroupParam,
                    SourceDirPathParam = data.SourceDirPathParam,
                    StorageAccountNameParam = data.StorageAccountNameParam,
                    SubscriptionParam = data.SubscriptionParam
                };


                var ret = await _jobHelper.StartExtractJobAsync(accessToken,ep);
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(true);
                return response;
            }
            catch (Exception ex)
            {
                var message = $"Could not process the video";
                _logger.LogError(message, "RunNovelCsamStartExtractJobAsync", ex);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }


        [Function("RunNovelCsamStartAnalyzeJobAsync")]
        public async Task<HttpResponseData> RunNovelCsamStartAnalyzeJobAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "jobs/analyze")] HttpRequestData req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<AnalyzeParameters>(requestBody);
                if (data == null)
                {
                    var message = $"Could not process the video, data is null";
                    _logger.LogError(message, "RunNovelCsamStartAnalyzeJobAsync", new Exception(message));
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }
                var fileName = data.FileNameParam;
                if (string.IsNullOrEmpty(fileName))
                {
                    var message = $"Could not process the video {fileName} was null or empty";
                    _logger.LogError(message, "RunNovelCsamStartAnalyzeJobAsync", new Exception(message));
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                _logger.LogInformation($"RunNovelCsamStartAnalyzeJobAsync Name: {fileName}");

                var accessToken = await AccountTokenProvider.GetAccountAccessTokenAsync(_logger).ConfigureAwait(false);

                var ret = await _jobHelper.StartAnalyzeJobAsync(accessToken,
                    new AnalyzeParameters
                    {
                        FileNameParam = data.FileNameParam,
                        KeyVaultParam = data.KeyVaultParam,
                        SourceContainerNameParam = data.SourceContainerNameParam,
                        SourceDirPathParam = data.SourceDirPathParam,
                        DestDirPathExtParam = data.DestDirPathExtParam,
                        DestDirPathParam = data.DestDirPathParam,
                        StorageAccountNameParam = data.StorageAccountNameParam,
                        CosmosDbNameParam = data.CosmosDbNameParam,
                        CosmosDbContainerNameParam = data.CosmosDbContainerNameParam,
                        DebugModeParam = data.DebugModeParam,
                        WriteToJsonParam = data.WriteToJsonParam,
                        WriteToCosmosParam = data.WriteToCosmosParam,
                        SubscriptionParam = data.SubscriptionParam,
                        ResourceGroupParam = data.ResourceGroupParam,
                        JobNameParam = data.JobNameParam


                    });

                //var ret = await _jobHelper.StartAnalyzeJobAsync(data.SubscriptionParam, 
                //    data.ResourceGroupParam,
                //    data.JobNameParam, 
                //    accessToken, 
                //    data.FileNameParam, 
                //    data.KeyVaultParam, 
                //    data.SourceContainerNameParam, 
                //    data.SourceDirPathParam,
                //    data.DestDirPathExtParam,
                //    data.DestDirPathParam,

                //    data.StorageAccountNameParam, 
                //    data.CosmosDbNameParam,data.CosmosDbContainerNameParam,
                //    data.DebugModeParam,
                //    data.WriteToJsonParam, 
                //    data.WriteToCosmosParam);
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(true);
                return response;
            }
            catch (Exception ex)
            {
                var message = $"Could not process the video";
                _logger.LogError(message, "RunNovelCsamStartAnalyzeJobAsync", ex);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

    }
}
