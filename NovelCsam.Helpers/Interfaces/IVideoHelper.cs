namespace NovelCsamDetection.Helpers.Interfaces
{
	public interface IVideoHelper
	{
		public Task UploadExtractedFramesToBlobAsync(int frameInterval, string filename, string containerName, string containerFolderPath, string containerFolderPathExtracted, string sourceFileNameOrPath);
		public Task UploadSegmentVideoToBlobAsync(int splitTime, string fileName, string containerName, string containerFolderPath, string containerFolderPathSegmented);
		public Task UploadFrameResultsAsync(string containerName, string containerFolderPath, string containerFolderPathResults, bool withBase64ofImage = false);
		public Task<string> UploadVideoToBlobAsync(string containerName, string containerFolderPath, string sourceFileNameOrPath, string containerFolderPostfix = "", bool isImages = false, string timestampIn = "", string customName = "");
	}
}