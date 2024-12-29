namespace NovelCsamDetection.Helper.Interfaces
{
    public interface IAzureContainerAppJobHelper
    {
        public Task<bool> StartExtractJobAsync(string subscriptionId, string resourceGroupName, string containerAppName, string jobName, string accessToken);
    }
}
