namespace NovelCsamDetection.Helpers
{
	public class JsonHelper : IJsonHelper
	{
		private readonly ILogHelper _logHelper;
		public JsonHelper(ILogHelper logHelper)
		{
			_logHelper = logHelper;
		}
		public string  WriteExtractedFramesToJson(string fileName, int frameInterval, List<string> extractedFrames, string newFilename)
		{
			var metadata = new ExtractVideoFrameJsonMetaData
			{
				SourceFile = fileName,
				FrameInterval = frameInterval,
				Frames = extractedFrames
			};

			string jsonFilename = $"{newFilename}.json";
			string localJsonPath = Path.Combine(Path.GetTempPath(), jsonFilename);

			var options = new JsonSerializerOptions
			{
				WriteIndented = true
			};

			try
			{
				string jsonString = JsonSerializer.Serialize(metadata, options);
				File.WriteAllText(localJsonPath, jsonString);
				_logHelper.LogInformation($"Metadata written to {localJsonPath}", nameof(IJsonHelper), nameof(WriteExtractedFramesToJson));
				return localJsonPath;
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred while writing to JSON file: {ex.Message}", nameof(IJsonHelper), nameof(WriteExtractedFramesToJson), ex);
				throw;
			}

		}
	}
}