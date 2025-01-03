namespace NovelCsamDetection.Helpers
{
	public class KeyVaultHelper
	{
		private readonly SecretClient _secretClient;
		private readonly ILogHelper _logHelper;

		public KeyVaultHelper(string keyVaultName, ILogHelper logHelper)
		{
			_logHelper = logHelper;
			var keyVaultUrl = $"https://{keyVaultName}.vault.azure.net/";
			_secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
		}
		public async Task<string> GetSecretAsync(string secretName)
		{
			try
			{
				KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
				return secret.Value;
			}
			catch (Exception ex)
			{
				_logHelper.LogException($"An error occurred while retrieving the secret: {ex.Message}", nameof(IKeyVaultHelper), nameof(GetSecretAsync), ex);
				throw;
			}
		}
	}
}