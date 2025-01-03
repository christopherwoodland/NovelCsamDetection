namespace NovelCsamDetection.Helpers.Interfaces
{
	public interface IVideoHelper
	{
		public Task UploadExtractedFramesToBlobAsync(int frameInterval, string filename, string containerName, string containerFolderPath, string containerFolderPathExtracted, string sourceFileNameOrPath);
		public Task UploadSegmentVideoToBlobAsync(int splitTime, string fileName, string containerName, string containerFolderPath, string containerFolderPathSegmented);
		public Task UploadFrameResultsToBlobAsync(string containerName, string containerFolderPath, string containerFolderPathResults, string sourceFileNameOrPath, bool withBase64ofImage = false);
		public Task UploadImageOnlyFrameResultsToBlobAsync(string containerName, string containerFolderPath, string containerFolderPathResults, string sourceFileNameOrPath, bool withBase64ofImage = false);
		public Task<string> UploadVideoToBlobAsync(string containerName, string containerFolderPath, string sourceFileNameOrPath);
	}
}