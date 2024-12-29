using Azure.Security.KeyVault.Secrets;
namespace NovelCsamDetection.Helpers.Interfaces
{
    public interface IKeyVaultHelper
    {
        public Task<string> GetSecretAsync(string secretName);
    }

}