namespace NovelCsamDetection.Helpers
{
	public class VideoHelper : IVideoHelper
	{
		private readonly IKernelBuilder _kernelBuilder;
		private readonly Kernel _kernel;
		private readonly ILogHelper _logHelper;
		private readonly IStorageHelper _sth;
		private readonly IContentSafteyHelper _csh;
		private readonly IAzureSQLHelper _ash;
		private enum FFMPEG_MODE { VSEG = 0, FSEG = 1 }
		private const string HATE = "hate";
		private const string SELF_HARM = "selfharm";
		private const string VIOLENCE = "violence";
		private const string SEXUAL = "sexual";


		public VideoHelper(IStorageHelper sth, ILogHelper logHelper, IContentSafteyHelper csh, IAzureSQLHelper ash)
		{
			_logHelper = logHelper;
			_sth = sth;
			_csh = csh;
			_kernelBuilder = Kernel.CreateBuilder();

			var oaidnm = Environment.GetEnvironmentVariable("OPEN_AI_DEPLOYMENT_NAME") ?? "";
			var oaikey = Environment.GetEnvironmentVariable("OPEN_AI_KEY") ?? "";
			var oaiendpoint = Environment.GetEnvironmentVariable("OPEN_AI_ENDPOINT") ?? "";
			var oaimodel = Environment.GetEnvironmentVariable("OPEN_AI_MODEL") ?? "";
			_kernelBuilder.AddAzureOpenAIChatCompletion(
				deploymentName: oaidnm,
				apiKey: oaikey,
				endpoint: oaiendpoint,
				modelId: oaimodel,
				serviceId: Guid.NewGuid().ToString());

			_kernel = _kernelBuilder.Build();
			_ash = ash;
		}

		public async Task<string> UploadFileToBlobAsync(string containerName, string containerFolderPath, string sourceFileNameOrPath, string containerFolderPostfix = "", bool isImages = false, string timestampIn = "", string customName = "")
		{
			string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
			if (!string.IsNullOrEmpty(timestampIn))
				timestamp = timestampIn;
			var ret = "";

			if (isImages)
			{
				var placeHolder = !string.IsNullOrEmpty(customName) ? customName : "images";
				if (!string.IsNullOrEmpty(containerFolderPostfix))
					ret = await _sth.UploadFileAsync(containerName, $"{containerFolderPath}/{placeHolder}/{timestamp}/{containerFolderPostfix}", sourceFileNameOrPath);
				else
				{
					ret = await _sth.UploadFileAsync(containerName, $"{containerFolderPath}/{placeHolder}/{timestamp}", sourceFileNameOrPath);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(containerFolderPostfix))
					ret = await _sth.UploadFileAsync(containerName, $"{containerFolderPath}/{Path.GetFileName(sourceFileNameOrPath)}/{timestamp}/{containerFolderPostfix}", sourceFileNameOrPath);
				else
				{
					ret = await _sth.UploadFileAsync(containerName, $"{containerFolderPath}/{Path.GetFileName(sourceFileNameOrPath)}/{timestamp}", sourceFileNameOrPath);
				}
			}

			return ret;
		}
		public async Task<string> UploadFrameResultsAsync(string containerName, string containerFolderPath, string containerFolderPathResults, bool withBase64ofImage = false)
		{
			var list = await _sth.ListBlobsInFolderWithResizeAsync(containerName, containerFolderPath, 3);
			Dictionary<string, IFrameResult>? ret = [];
			var runId = Guid.NewGuid().ToString();
			var runDateTime = DateTime.UtcNow;
			foreach (var item in list)
			{
				AnalyzeImageResult? air = GetContentSafteyDetails(item.Value);
				var summary = await SummarizeImageAsync(item.Value, "Can you do a detail analysis and tell me all the minute details about this image. Use no more than 450 words!!!");
				var childYesNo = await SummarizeImageAsync(item.Value, "Is there a younger person or child in this image? If you can't make a determination ANSWER No, ONLY ANSWER Yes or No!!");
				var md5Hash = CreateMD5Hash(item.Value);
				var newItem = new FrameResult
				{
					MD5Hash = md5Hash,
					Summary = summary,
					RunId = runId,
					Id = Guid.NewGuid().ToString(),
					Frame = item.Key,
					ChildYesNo = childYesNo,
					ImageBase64 = withBase64ofImage ? ConvertToBase64(item.Value) : "",
					RunDateTime = runDateTime
				};

				if (air != null)
				{
					foreach (var citem in air.CategoriesAnalysis)
					{
						if (citem.Category.ToString().ToLowerInvariant() == HATE)
						{
							newItem.Hate = (int)citem.Severity;
						}
						else if (citem.Category.ToString().ToLowerInvariant() == SELF_HARM)
						{
							newItem.SelfHarm = (int)citem.Severity;
						}
						else if (citem.Category.ToString().ToLowerInvariant() == VIOLENCE)
						{
							newItem.Violence = (int)citem.Severity;
						}
						else if (citem.Category.ToString().ToLowerInvariant() == SEXUAL)
						{
							newItem.Sexual = (int)citem.Severity;
						}
					}
				}
				//ret.Add(item.Key, newItem);
				//_cdbh.CreateFrameResult(newItem); //CosmosDB Write
				await _ash.CreateFrameResult(newItem);
				await _ash.InsertBase64(newItem);
			}
			return runId;
		}
		public string ConvertToBase64(BinaryData imageData)
		{
			byte[] imageBytes = imageData.ToArray();
			return Convert.ToBase64String(imageBytes);
		}
		public string CreateMD5Hash(BinaryData imageData)
		{
			using MD5 md5 = MD5.Create();
			byte[] hashBytes = md5.ComputeHash(imageData.ToArray());
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hashBytes.Length; i++)
			{
				sb.Append(hashBytes[i].ToString("x2"));
			}
			return sb.ToString();
		}

		public async Task<string> SummarizeImageAsync(BinaryData imageBytes, string userPrompt)
		{
			const int maxRetries = 3;
			const int delayMilliseconds = 2000;

			// Define a Polly retry policy
			var retryPolicy = Policy
				.Handle<HttpRequestException>(ex => ex.StatusCode == (HttpStatusCode)429)
				.WaitAndRetryAsync(maxRetries, retryAttempt => TimeSpan.FromMilliseconds(delayMilliseconds),
					(exception, timeSpan, retryCount, context) =>
					{
						_logHelper.LogInformation($"Retry {retryCount}/{maxRetries} after receiving 429 Too Many Requests. Waiting {timeSpan.TotalMilliseconds}ms before retrying.",
							nameof(VideoHelper), nameof(SummarizeImageAsync));
					});

			try
			{
				return await retryPolicy.ExecuteAsync(async () =>
				{
					var chat = _kernel.GetRequiredService<IChatCompletionService>();
					var history = new ChatHistory();
					history.AddSystemMessage("You are a helpful assistant that responds to questions directly");
					var message = new ChatMessageContentItemCollection
			{
				new TextContent(userPrompt),
				new ImageContent(imageBytes, GetMimeType(imageBytes))
			};

					history.AddUserMessage(message);
					var res = await chat.GetChatMessageContentAsync(history);
					return res.Content ?? "NO SUMMARY GENERATED";
				});
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred summarizing an image: {ex.Message}", nameof(VideoHelper), nameof(SummarizeImageAsync), ex);
				return $"Error: {ex.Message}";
			}
		}
		public AnalyzeImageResult? GetContentSafteyDetails(BinaryData bd)
		{
			return _csh.AnalyzeImage(bd);
		}

		public async Task<bool> UploadExtractedFramesToBlobAsync(int frameInterval, string fileName, string containerName, string containerFolderPath, string containerFolderPathExtracted, string sourceFileNameOrPath)
		{
			// Generate a new GUID
			string guid = Guid.NewGuid().ToString();

			// Create a unique directory in the temporary path using the GUID
			string tempPath = Path.Combine(Path.GetTempPath(), guid);

			// Ensure the directory exists
			if (!Directory.Exists(tempPath))
			{
				Directory.CreateDirectory(tempPath);
			}

			// Combine the temporary directory path with the file name
			string localVideoPath = Path.Combine(tempPath, fileName);
			await _sth.DownloadFileAsync(containerName, containerFolderPath, fileName, localVideoPath);

			var extractedFrames = await ExtractFramesAsync(localVideoPath, frameInterval, fileName);
			string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
			foreach (var frame in extractedFrames)
			{
				string fileNameFrame = Path.GetFileName(frame);
				await _sth.UploadFileAsync(containerName, containerFolderPathExtracted, fileNameFrame, localVideoPath, timestamp, sourceFileNameOrPath);
				_logHelper.LogInformation($"Uploaded {fileNameFrame} to {timestamp}", nameof(VideoHelper), nameof(UploadExtractedFramesToBlobAsync));

			}
			return true;
		}
		public async Task UploadSegmentVideoToBlobAsync(int splitTime, string fileName, string containerName, string containerFolderPath, string containerFolderPathSegmented)
		{
			// Generate a new GUID
			string guid = Guid.NewGuid().ToString();

			// Create a unique directory in the temporary path using the GUID
			string tempPath = Path.Combine(Path.GetTempPath(), guid);

			// Ensure the directory exists
			if (!Directory.Exists(tempPath))
			{
				Directory.CreateDirectory(tempPath);
			}

			// Combine the temporary directory path with the file name
			string localVideoPath = Path.Combine(tempPath, fileName);
			await _sth.DownloadFileAsync(containerName, containerFolderPath, fileName, localVideoPath);
			var segmentedVideos = await SegmentVideoAsync(localVideoPath, splitTime);


			string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
			foreach (var video in segmentedVideos)
			{
				string fileNameVideo = Path.GetFileName(video);
				await _sth.UploadFileAsync(containerName, containerFolderPathSegmented, fileNameVideo, localVideoPath, timestamp, fileName);
				_logHelper.LogInformation($"Uploaded {fileNameVideo} to {timestamp}", nameof(VideoHelper), nameof(UploadSegmentVideoToBlobAsync));

			}
		}

		#region Private Methods

		private string GetMimeType(BinaryData imageData)
		{
			using (var ms = new MemoryStream(imageData.ToArray()))
			{
				using (var image = Image.FromStream(ms))
				{
					ImageFormat format = image.RawFormat;
					if (ImageFormat.Jpeg.Equals(format))
						return "image/jpeg";
					if (ImageFormat.Png.Equals(format))
						return "image/png";
					if (ImageFormat.Gif.Equals(format))
						return "image/gif";
					if (ImageFormat.Bmp.Equals(format))
						return "image/bmp";
					if (ImageFormat.Tiff.Equals(format))
						return "image/tiff";
					if (ImageFormat.Icon.Equals(format))
						return "image/x-icon";
					if (ImageFormat.Emf.Equals(format))
						return "image/emf";
					if (ImageFormat.Exif.Equals(format))
						return "image/exif";
					if (ImageFormat.Wmf.Equals(format))
						return "image/wmf";
					if (ImageFormat.MemoryBmp.Equals(format))
						return "image/bmp"; // MemoryBmp is treated as BMP

					// Default to a generic binary stream if format is unknown
					return "application/octet-stream";
				}
			}
		}
		private async Task<List<string>> ExtractFramesAsync(string videoPath, int frameInterval = 1, string filename = "frame")
		{
			try
			{
				var outputDir = Path.GetDirectoryName($"{videoPath}");
				if (!Directory.Exists(outputDir))
				{
					Directory.CreateDirectory(outputDir);
				}
				await RunFFmpegAsync(videoPath, outputDir, null, FFMPEG_MODE.FSEG, 1);
				List<string> extractedFrames = [.. Directory.GetFiles(outputDir, "*.jpg")];
				extractedFrames.Sort();
				return extractedFrames;
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred while extracting frames: {ex.Message}", nameof(VideoHelper), nameof(ExtractFramesAsync), ex);
				return [];
			}
		}
		private async Task<List<string>> SegmentVideoAsync(string videoPath, int splitTime)
		{
			try
			{
				var direcName = Path.GetDirectoryName(videoPath) ?? throw new NullReferenceException("SegmentVideoAsync outputDir is null");
				string outputDir = Path.Combine(direcName, "output");
				if (!Directory.Exists(outputDir))
				{
					Directory.CreateDirectory(outputDir);
				}
				var segmentDuration = TimeSpan.FromMinutes(splitTime);
				await RunFFmpegAsync(videoPath, outputDir, segmentDuration, FFMPEG_MODE.VSEG);
				List<string> segmentedVideos = [.. Directory.GetFiles(outputDir, "*.mkv")];
				segmentedVideos.Sort();
				return segmentedVideos;
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred while segmenting video: {ex.Message}", nameof(VideoHelper), nameof(SegmentVideoAsync), ex);
				return [];
			}
		}
		private async Task RunFFmpegAsync(string videoPath, string outputFilePath, TimeSpan? segmentDuration, FFMPEG_MODE mode = 0, int frameInterval = 1)
		{
			try
			{
				FFmpeg.SetExecutablesPath(@"ffmpeg-full\tools\ffmpeg\bin");
				var conversion = FFmpeg.Conversions.New();
				if (mode == FFMPEG_MODE.VSEG)
				{
					//Video segmentation
					string framePattern = Path.Combine(outputFilePath, $"output_%010d.mkv");
					conversion.AddParameter($"-i \"{videoPath}\"")
				  .AddParameter($"-c:v ffv1")
				  .AddParameter($"-c:a copy")
				  .AddParameter($"-map 0")
				  .AddParameter($"-segment_time {segmentDuration.Value.TotalSeconds}")
				  //.AddParameter($"-force_key_frames \"expr:gte(t,n_forced*{segmentDuration})\"")
				  .AddParameter($"-force_key_frames \"expr:gte(t,n_forced*{segmentDuration.Value.TotalSeconds})\"")

				  .AddParameter($"-f segment")
				  .AddParameter($"-reset_timestamps 1")
				  .SetOutput(framePattern);

				}
				else if (mode == FFMPEG_MODE.FSEG)
				{
					//Frame segmentaion
					string framePattern = Path.Combine(outputFilePath, $"frame_%010d.jpg");
					conversion.AddParameter($"-i \"{videoPath}\"")
					.AddParameter($"-vf \"fps=1/{frameInterval}\"")
					.AddParameter($"-compression_level 0")
					//.AddParameter($"-q:v 2") // Adjust the quality to control the file size
					.AddParameter($"-fs 4194304") // Set the maximum file size to 4 MB
					.SetOutput(framePattern);
				}

				// Start the conversion process
				IConversionResult result = await conversion.Start();

				// Log the output and error
				_logHelper.LogInformation($"File {videoPath}: {result.Arguments}", nameof(VideoHelper), nameof(RunFFmpegAsync));
				_logHelper.LogInformation($"File {videoPath}: {result}", nameof(VideoHelper), nameof(RunFFmpegAsync));
			}
			catch
			{

				throw;
			}
		}

		#endregion Private Methods
	}
}