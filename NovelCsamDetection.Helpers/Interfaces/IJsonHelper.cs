namespace NovelCsamDetection.Helpers.Interfaces
{
	public interface IJsonHelper
	{
		public string WriteExtractedFramesToJson(string fileName, int frameInterval, List<string> extractedFrames, string newFilename);
	}
}