using Microsoft.VisualBasic;
using System.IO.Abstractions;
using Xabe.FFmpeg;

namespace NovelCsamDetection.Helpers
{
	public class VideoHelper : IVideoHelper
	{
		private readonly DataLakeServiceClient _serviceClient;
		private readonly ILogHelper _logHelper;
		private List<string> _extractedFrames;
		private readonly IStorageHelper _sth;
		private enum FFMPEG_MODE { VSEG = 0, FSEG = 1 }

		public VideoHelper(IStorageHelper sth, ILogHelper logHelper)
		{
			_logHelper = logHelper;
			_extractedFrames = [];
			_sth = sth;
		}

		public async Task UploadExtractedFramesToBlobAsync(int frameInterval, string fileName, string containerName, string containerFolderPath, string containerFolderPathExtracted)
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

			_extractedFrames = await ExtractFramesAsync(localVideoPath, frameInterval, fileName);
			string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
			foreach (var frame in _extractedFrames)
			{
				string fileNameFrame = Path.GetFileName(frame);
				await _sth.UploadFileAsync(containerName, containerFolderPathExtracted, fileNameFrame, localVideoPath, timestamp, fileName);
				_logHelper.LogInformation($"Uploaded {fileNameFrame} to {timestamp}", nameof(VideoHelper), nameof(UploadExtractedFramesToBlobAsync));

			}
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
				List<string> extractedFrames = [.. Directory.GetFiles(outputDir, "*.png")];
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
			catch (Exception ex){
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
					string framePattern = Path.Combine(outputFilePath, $"frame_%010d.png");
					conversion.AddParameter($"-i \"{videoPath}\"")
					.AddParameter($"-vf \"fps=1/{frameInterval}\"")
					.AddParameter($"-compression_level 0")
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