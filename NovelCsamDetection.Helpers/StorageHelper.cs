
namespace NovelCsamDetection.Helpers
{
	public class StorageHelper : IStorageHelper
	{
		private readonly DataLakeServiceClient _serviceClient;
		private readonly ILogHelper _logHelper;

		public StorageHelper(ILogHelper logHelper)
		{
			_logHelper = logHelper;

			var san = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME")??"";
			var sak = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY") ?? "";
			var saurl = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_URL") ?? "";

			string serviceUri = saurl;
			_serviceClient = new DataLakeServiceClient(new Uri(serviceUri), 
				new StorageSharedKeyCredential(san, sak));
		}

		public StorageHelper(string storageAccountName, string storageAccountKey, ILogHelper logHelper)
		{
			_logHelper = logHelper;
			string serviceUri = $"https://{storageAccountName}.dfs.core.windows.net";
			_serviceClient = new DataLakeServiceClient(new Uri(serviceUri), new StorageSharedKeyCredential(storageAccountName, storageAccountKey));
		}

		#region Private Methods
		private async Task<BinaryData> GetFileAsBinaryDataWithResizeAsync(string fileName, string containerName, string folderPath, int maxSize)
		{
			DataLakeFileSystemClient fileSystemClient = _serviceClient.GetFileSystemClient(containerName);
			DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(folderPath);
			DataLakeFileClient fileClient = directoryClient.GetFileClient(Path.GetFileName(fileName));

			Azure.Response<FileDownloadInfo> downloadResponse = await fileClient.ReadAsync();

			using var memoryStream = new MemoryStream();
			await downloadResponse.Value.Content.CopyToAsync(memoryStream);
			memoryStream.Position = 0; // Reset the stream position to the beginning

			if (memoryStream.Length >= maxSize)
			{
				using var originalImage = Image.FromStream(memoryStream);
				var resizedImage = ResizeImage(originalImage, maxSize);
				using var resizedStream = new MemoryStream();
				resizedImage.Save(resizedStream, ImageFormat.Jpeg);
				return new BinaryData(resizedStream.ToArray());
			}

			return new BinaryData(memoryStream.ToArray());
		}

		private Image ResizeImage(Image image, int maxSize)
		{
			int newWidth;
			int newHeight;

			if (image.Width > image.Height)
			{
				newWidth = maxSize;
				newHeight = (int)(image.Height * (maxSize / (double)image.Width));
			}
			else
			{
				newHeight = maxSize;
				newWidth = (int)(image.Width * (maxSize / (double)image.Height));
			}

			var resizedImage = new Bitmap(newWidth, newHeight);
			using (var graphics = Graphics.FromImage(resizedImage))
			{
				graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				graphics.DrawImage(image, 0, 0, newWidth, newHeight);
			}

			return resizedImage;
		}

		private async Task<BinaryData> GetFileAsBinaryDataAsync(string fileName, string containerName, string folderPath)
		{
			DataLakeFileSystemClient fileSystemClient = _serviceClient.GetFileSystemClient(containerName);
			DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(folderPath);
			DataLakeFileClient fileClient = directoryClient.GetFileClient(Path.GetFileName((fileName)));

			Azure.Response<FileDownloadInfo> downloadResponse = await fileClient.ReadAsync();

			using var memoryStream = new MemoryStream();
			await downloadResponse.Value.Content.CopyToAsync(memoryStream);
			memoryStream.Position = 0; // Reset the stream position to the beginning
			return new BinaryData(memoryStream.ToArray());
		}

		#endregion


		#region Public Methods
		public async Task<Dictionary<string, BinaryData>> ListBlobsInFolderWithResizeAsync(string containerName, string folderPath)
		{
			var ret = new Dictionary<string, BinaryData>();
			try
			{
				DataLakeFileSystemClient fileSystemClient = _serviceClient.GetFileSystemClient(containerName);
				DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(folderPath);

				await foreach (PathItem pathItem in directoryClient.GetPathsAsync())
				{
					if ((bool)!pathItem.IsDirectory)
					{
						var binaryData = await GetFileAsBinaryDataWithResizeAsync(pathItem.Name, containerName, folderPath, 4194304);
						ret.Add(pathItem.Name, binaryData);
					}
				}
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred listing blobs: {ex.Message}", nameof(StorageHelper), nameof(ListBlobsInFolderAsync), ex);
				throw;
			}

			return ret;
		}
		public async Task<Dictionary<string, BinaryData>> ListBlobsInFolderAsync(string containerName, string folderPath)
		{
			var ret = new Dictionary<string, BinaryData>();
			try
			{
				DataLakeFileSystemClient fileSystemClient = _serviceClient.GetFileSystemClient(containerName);
				DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(folderPath);

				await foreach (PathItem pathItem in directoryClient.GetPathsAsync())
				{
					if ((bool)!pathItem.IsDirectory)
					{
						var binaryData = await GetFileAsBinaryDataAsync(pathItem.Name, containerName, folderPath);
						ret.Add(pathItem.Name, binaryData);
					}
				}
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred listing blobs: {ex.Message}", nameof(StorageHelper), nameof(ListBlobsInFolderAsync), ex);
				throw;
			}

			return ret;
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
					string lvp = Path.Combine(direc, fileName);
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

		public async Task DownloadFileAsync(string containerName, string containerFolderPath, 
			string fileName, string localVideoPath)
		{
			try
			{
				DataLakeFileSystemClient fileSystemClient = _serviceClient.GetFileSystemClient(containerName);
				DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(containerFolderPath);
				DataLakeFileClient fileClient = directoryClient.GetFileClient(fileName);

				Azure.Response<FileDownloadInfo> downloadInfo = await fileClient.ReadAsync();
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

		#endregion 
	}
}