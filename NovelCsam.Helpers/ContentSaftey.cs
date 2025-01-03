using Polly;
using System.Net;

namespace NovelCsamDetection.Helpers
{
	public class ContentSafteyHelper : IContentSafteyHelper
	{
		private readonly ILogHelper _logHelper;
		private readonly ContentSafetyClient _csc;
		public ContentSafteyHelper(ILogHelper logHelper)
		{
			_logHelper = logHelper;
			var cscs = Environment.GetEnvironmentVariable("CONTENT_SAFETY_CONNECTION_STRING") ?? "";
			var csck = Environment.GetEnvironmentVariable("CONTENT_SAFETY_CONNECTION_KEY") ?? "";
			_csc = new ContentSafetyClient(new Uri(cscs), new Azure.AzureKeyCredential(csck));
		}
		public AnalyzeImageResult? AnalyzeImage(BinaryData inputImage)
		{
			const int maxRetries = 3;
			const int delayMilliseconds = 2000;

			// Define a Polly retry policy
			var retryPolicy = Policy
				.Handle<HttpRequestException>(ex => ex.StatusCode == (HttpStatusCode)429)
				.WaitAndRetry(maxRetries, retryAttempt => TimeSpan.FromMilliseconds(delayMilliseconds),
					(exception, timeSpan, retryCount, context) =>
					{
						_logHelper.LogInformation($"Retry {retryCount}/{maxRetries} after receiving 429 Too Many Requests. Waiting {timeSpan.TotalMilliseconds}ms before retrying.",nameof(ContentSafteyHelper),nameof(AnalyzeImage));
					});

			try
			{
				return retryPolicy.Execute(() =>
				{
					if (_csc != null)
					{
						ContentSafetyImageData image = new(inputImage);
						var request = new AnalyzeImageOptions(image);
						var response = _csc.AnalyzeImage(request);
						return response;
					}
					else
					{
						return null;
					}
				});
			}
			catch (Exception ex)
			{
				_logHelper.LogException(ex.Message, nameof(ContentSafteyHelper), nameof(AnalyzeImage), ex);
				return null;
			}
		}
	}
}
