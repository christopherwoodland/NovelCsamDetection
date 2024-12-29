using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Files.DataLake;
namespace NovelCsamDetection.Helpers.Interfaces
{
	public interface IStorageHelper
	{
		public Task UploadFileAsync(string containerName, string folderPath, string fileName, string localVideoPath, string timestamp, string originalFileName);
		public Task DownloadFileAsync(string containerName, string containerFolderPath, string fileName, string localVideoPath);
		}

}