
using Azure;
using Azure.Storage.Files.DataLake.Models;

namespace NovelCsamDetection.Helpers
{
	public class StorageHelper : IStorageHelper
	{
		private readonly DataLakeServiceClient _serviceClient;
		private readonly ILogHelper _logHelper;

		public StorageHelper(ILogHelper logHelper)
		{
			_logHelper = logHelper;
			string serviceUri = $"https://digitalforensicsstg.dfs.core.windows.net";
			_serviceClient = new DataLakeServiceClient(new Uri(serviceUri), new StorageSharedKeyCredential("digitalforensicsstg", "IGnMXV0C6jZrPX3RapNgHDtfY0OI8M20Y9K+qOG6Zm0mvvRdzUJOHHIkae0VjTKoIWRugb82cFi7+AStRuP/MQ=="));
		}

		public StorageHelper(string storageAccountName, string storageAccountKey, ILogHelper logHelper)
		{
			_logHelper = logHelper;
			string serviceUri = $"https://{storageAccountName}.dfs.core.windows.net";
			_serviceClient = new DataLakeServiceClient(new Uri(serviceUri), new StorageSharedKeyCredential(storageAccountName, storageAccountKey));
		}

		public async Task UploadFileAsync(string containerName, string folderPath, string fileName, string localVideoPath, string timestamp, string originalFileName)
		{
			try
			{
				DataLakeFileSystemClient fileSystemClient = _serviceClient.GetFileSystemClient(containerName);
				DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient($"{folderPath}/{originalFileName}/{timestamp}");
				DataLakeFileClient fileClient = directoryClient.GetFileClient(fileName);
				var direc = Path.GetDirectoryName(localVideoPath);
				//DataLakeFileUploadOptions uploadOptions = new()
				//{
				//	// If change notifications are enabled, set Close to true
				//	// This value indicates the final close of the file stream
				//	// And emits a change notification event upon successful flush
					
				//	Close = true,
				//	Conditions = { 
				//	}
				//};
				if (direc != null)
				{
					string lvp = Path.Combine(direc, "output", fileName);
					FileStream fileStream = File.OpenRead(lvp);

					var fur = await fileClient.UploadAsync(fileStream, true);
					_logHelper.LogInformation($"File '{fileName}' uploaded to '{folderPath} {fur}' in container '{containerName}'.", nameof(StorageHelper), nameof(UploadFileAsync));
				}
				else
				{
					var message = $"An error occurred while uploading the file: {fileName}. Direc is null";
					var arge = new ArgumentNullException(message);
					_logHelper.LogException(message, nameof(StorageHelper), nameof(UploadFileAsync), arge);
					throw arge;
				}
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred while uploading the file: {ex.Message}", nameof(StorageHelper), nameof(UploadFileAsync), ex);
				throw;
			}
		}

		public async Task DownloadFileAsync(string containerName, string containerFolderPath, string fileName, string localVideoPath)
		{
			try
			{
				DataLakeFileSystemClient fileSystemClient = _serviceClient.GetFileSystemClient(containerName);
				DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(containerFolderPath);
				DataLakeFileClient fileClient = directoryClient.GetFileClient(fileName);

				Response<FileDownloadInfo> downloadInfo = await fileClient.ReadAsync();
				using (FileStream fs = File.OpenWrite(localVideoPath))
				{
					await downloadInfo.Value.Content.CopyToAsync(fs);
					fs.Close();
				}
				_logHelper.LogInformation($"File '{fileName}' downloaded to '{localVideoPath}' in container '{containerName}'.", nameof(StorageHelper), nameof(UploadFileAsync));
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred while downloading the file: {ex.Message}", nameof(StorageHelper), nameof(DownloadFileAsync), ex);
				throw;
			}
		}
	}
}