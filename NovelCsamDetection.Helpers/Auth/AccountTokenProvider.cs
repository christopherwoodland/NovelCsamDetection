namespace NovelCsamDetection.Helpers.Auth
{
    public static class AccountTokenProvider
    {
        public static async Task<string> GetAccountAccessTokenAsync(ILogger logger, ArmAccessTokenPermission permission = ArmAccessTokenPermission.Contributor, ArmAccessTokenScope scope = ArmAccessTokenScope.Account, CancellationToken ct = default)
        {
            var armAccessToken = await GetArmAccessTokenAsync(ct);
            return armAccessToken;
        }

        public static async Task<string> GetArmAccessTokenAsync(CancellationToken ct = default)
        {
            var azureResourceManager = "https://management.azure.com";
            var tokenRequestContext = new TokenRequestContext(new[] { $"{azureResourceManager}/.default" });
            var tokenRequestResult = await new DefaultAzureCredential().GetTokenAsync(tokenRequestContext, ct);
            return tokenRequestResult.Token;
        }
    }
}
