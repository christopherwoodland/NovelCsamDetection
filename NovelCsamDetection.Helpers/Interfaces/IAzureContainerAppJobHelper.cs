using NovelCsam.Models;

namespace NovelCsamDetection.Helpers.Interfaces
{
    public interface IAzureContainerAppJobHelper
    {
        public Task<bool> StartExtractJobAsync(string accessToken, ExtractParameters ep);
        public Task<bool> StartAnalyzeJobAsync(string accessToken, AnalyzeParameters aprams);
    }
}
