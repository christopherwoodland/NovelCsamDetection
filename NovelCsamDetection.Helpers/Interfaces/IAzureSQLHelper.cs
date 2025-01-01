namespace NovelCsamDetection.Helpers.Interfaces
{
	public interface IAzureSQLHelper
	{
		public Task<IFrameResult?> CreateFrameResult(IFrameResult item);
		public Task<IFrameResult?> InsertBase64(IFrameResult item);
	}
}
