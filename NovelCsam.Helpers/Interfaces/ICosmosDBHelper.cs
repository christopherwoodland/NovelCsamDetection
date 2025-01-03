namespace NovelCsamDetection.Helpers.Interfaces
{
	public interface ICosmosDBHelper
	{
		public Task<IFrameResult?> CreateFrameResult(IFrameResult item);
	}
}
