namespace NovelCsamDetection.Helpers.Interfaces
{
	public interface IVideoHelper
	{
		public Task UploadExtractedFramesToBlobAsync(int frameInterval, string filename, string containerName, string containerFolderPath, string containerFolderPathExtracted);
		public Task UploadSegmentVideoToBlobAsync(int splitTime, string fileName, string containerName, string containerFolderPath, string containerFolderPathSegmented);
	}
}