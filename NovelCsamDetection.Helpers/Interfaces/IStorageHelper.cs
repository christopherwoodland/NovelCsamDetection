namespace NovelCsamDetection.Helpers.Interfaces
{
	public interface IStorageHelper
	{
		public Task UploadFileAsync(string containerName, string folderPath, string fileName, string localVideoPath, string timestamp, string originalFileName);
		public Task DownloadFileAsync(string containerName, string containerFolderPath, string fileName, string localVideoPath);
		public Task<Dictionary<string, BinaryData>> ListBlobsInFolderAsync(string containerName, string folderPath);
		public Task<Dictionary<string, BinaryData>> ListBlobsInFolderWithResizeAsync(string containerName, string folderPath);
	}
}